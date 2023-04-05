namespace Raki.TelegramBot.API.Examples;

using Microsoft.AspNetCore.Mvc;

public class SecurityIssueExample : Controller
{
    public IActionResult CreateCookie()
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTime.Now.AddDays(30),
            Secure = false
        };

        this.Response.Cookies.Append("MyCookieName", "MyCookieValue", cookieOptions);

        return Content("Cookie created!");
    }
}
