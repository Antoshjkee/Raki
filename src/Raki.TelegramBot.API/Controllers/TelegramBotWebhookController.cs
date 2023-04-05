namespace Raki.TelegramBot.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Raki.TelegramBot.API.Models;
using System.Text;
using Raki.TelegramBot.API.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

[ApiController]
[Route("/api/webhook")]
public class TelegramBotWebhookController : ControllerBase
{
    private readonly TelegramBot _telegramBot;
    private readonly IOptions<BotOptions> _botConfig;
    private readonly BotCommandService _botCommandService;
    private readonly StorageService _storageService;

    public TelegramBotWebhookController(Services.TelegramBot telegramBot, IOptions<BotOptions> botConfig,
        BotCommandService botCommandService, StorageService storageService)
    {
        _telegramBot = telegramBot;
        _botConfig = botConfig;
        _botCommandService = botCommandService;
        _storageService = storageService;
    }

    [HttpGet("setup")]
    public async Task<IActionResult> Setup()
    {
        await _telegramBot.Client.SetWebhookAsync(_botConfig.Value.WebhookUrl, allowedUpdates: Array.Empty<UpdateType>());
        return Ok($"Webhook : '{_botConfig.Value.WebhookUrl}' has been setup");
    }

    [HttpGet("info")]
    public async Task<IActionResult> Test()
    {
        var info = await _telegramBot.Client.GetWebhookInfoAsync();
        return Ok(info);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage()
    {
        using var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
        var requestBody = await reader.ReadToEndAsync();

        var update = JsonConvert.DeserializeObject<Update>(requestBody);
        if (update == null) return BadRequest();

        var result = update.Type switch
        {
            UpdateType.Message => await ProcessMessageAsync(update.Message),
            UpdateType.CallbackQuery => await ProcessCallbackQueryAsync(update.CallbackQuery),
            _ => Ok(),
        };

        return result;

        return Ok();
    }

    private async Task<IActionResult> ProcessMessageAsync(Message message)
    {
        if (message == null) return Ok();

        try
        {
            var commandAttempt = _botCommandService.TryGetCommand(message.Text, out var command);
            if (commandAttempt)
            {
                var commandResponse = await command!.ProcessAsync(message);
                await _telegramBot.Client.SendTextMessageAsync(message.Chat.Id,
                    commandResponse.ResponseMessage,
                    parseMode: commandResponse.Mode,
                    replyMarkup: commandResponse.Keyboard,
                    replyToMessageId: commandResponse.ReplyToId);
            }

            return Ok();
        }
        catch (Exception exception)
        {
            await _telegramBot.Client.SendTextMessageAsync(message.Chat.Id, "Exception : " + exception.Message);
            return BadRequest(exception.Message);
        }
    }

    private async Task<IActionResult> ProcessCallbackQueryAsync(CallbackQuery callbackQuery)
    {
        if (callbackQuery == null) return Ok();

        try
        {
            var callbackCommandAttempt = _botCommandService.TryGetCallbackCommand(callbackQuery.Data, out var callbackCommand);
            if (callbackCommandAttempt)
            {
                var response = await callbackCommand!.ProcessAsync(callbackQuery);
            }

            return Ok();
        }
        catch (Exception exception)
        {
            await _telegramBot.Client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Exception : " + exception.Message);
            return BadRequest(exception.Message);
        }

    }
}
