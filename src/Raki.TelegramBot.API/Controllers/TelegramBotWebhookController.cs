using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Raki.TelegramBot.API.Models;
using System.Text;
using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Raki.TelegramBot.API.Controllers;

[ApiController]
[Route("/api/webhook")]
public class TelegramBotWebhookController : ControllerBase
{
    private readonly Services.TelegramBot _telegramBot;
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

    [HttpPost]
    public async Task<IActionResult> SendMessage()
    {
        using var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
        var requestBody = await reader.ReadToEndAsync();

        var update = JsonConvert.DeserializeObject<Update>(requestBody);

        if (update == null) return BadRequest();
        if (update.Type != UpdateType.Message) return Ok();

        var message = update.Message;
        if (message == null) return Ok();

        var commandAttempt = _botCommandService.TryGetCommand(message.Text, out var command);
        if (commandAttempt)
        {
            var processResult = await command!.ProcessAsync(message);
            await _telegramBot.Client.SendTextMessageAsync(message.Chat.Id, processResult, command.Mode);
        }

        return Ok();
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        var info = await _telegramBot.Client.GetWebhookInfoAsync();
        return Ok(info);
    }

    [HttpGet("adduser")]
    public async Task<IActionResult> Test(string userName)
    {
        await _storageService.AddPlayerAsync(new PlayerRecordEntity
        {
            PartitionKey = "123",
            UserName = userName,
            RowKey = Guid.NewGuid().ToString(),
        });

        return Ok();
    }
}
