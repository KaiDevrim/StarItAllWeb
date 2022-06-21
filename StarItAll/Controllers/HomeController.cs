namespace StarItAll.Controllers;

using Microsoft.AspNetCore.Mvc;
using Models;
using Octokit;
using Activity = System.Diagnostics.Activity;

public class HomeController : Controller
{
    private readonly GitHubClient _client = new(new ProductHeaderValue("StarItAllWeb"));
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _config;

    public HomeController(ILogger<HomeController> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }
    public async Task<ActionResult> Index()
    {
        var accessToken = HttpContext.Session.GetString("OAuthToken");
        if (accessToken != null) _client.Credentials = new Credentials(accessToken);

        try
        {
            var repos = await _client.Repository.GetAllForUser("KaiDevrim");
            return View(repos);
        }
        catch (AuthorizationException)
        {
            return Redirect(GetOauthLoginUrl());
        }
    }

    public async Task<ActionResult> Authorize(string code)
    {
        if (!string.IsNullOrEmpty(code))
        {
            var token = await _client.Oauth.CreateAccessToken(
                new OauthTokenRequest(_config["ClientId"], _config["ClientSecret"], code));
            HttpContext.Session.SetString("OAuthToken", token.AccessToken);
        }

        return RedirectToAction("Index");
    }

    private string GetOauthLoginUrl()
    {
        HttpContext.Session.SetString("CSRF:State", _config["CSRF"]);

        // 1. Redirect users to request GitHub access
        var request = new OauthLoginRequest(_config["ClientId"])
        {
            Scopes = {"user", "notifications"},
            State = _config["CSRF"]
        };
        var oauthLoginUrl = _client.Oauth.GetGitHubLoginUrl(request);
        return oauthLoginUrl.ToString();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}