namespace StarItAll.Controllers;

using Microsoft.AspNetCore.Mvc;
using Models;
using Octokit;
using Activity = System.Diagnostics.Activity;

public class HomeController : Controller
{
    private readonly GitHubClient _client = new(new ProductHeaderValue("StarItAllWeb"));
    private readonly IConfiguration _config;
    private readonly ILogger<HomeController> _logger;
    private readonly IndexViewModel model = new();

    public HomeController(ILogger<HomeController> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<ActionResult> Index()
    {
        var accessToken = HttpContext.Session.GetString("OAuthToken");
        if (accessToken != null) _client.Credentials = new Credentials(accessToken);
        if (accessToken == null) return Redirect(GetOauthLoginUrl());
        try
        {
            model.ErrorMessage = string.Empty;
            return View(model);
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
            Scopes = {"public_repo"},
            State = _config["CSRF"]
        };
        var oauthLoginUrl = _client.Oauth.GetGitHubLoginUrl(request);
        return oauthLoginUrl.ToString();
    }

    [HttpPost]
    public async Task<ActionResult> Index([FromForm] string user)
    {
        var accessToken = HttpContext.Session.GetString("OAuthToken");
        if (accessToken != null) _client.Credentials = new Credentials(accessToken);
        if (accessToken == null) return Redirect(GetOauthLoginUrl());
        try
        {
            var repos = await _client.Repository.GetAllForUser(user);
            foreach (var repo in repos) await _client.Activity.Starring.StarRepo(user, repo.Name);

            model.Username = user;
            model.AvatarUrl = _client.User.Get(user).Result.AvatarUrl;
            model.Repositories = repos;
            model.ErrorMessage = null;
            return View(model);
        }
        catch (NotFoundException)
        {
            model.ErrorMessage = "User not found";
            return View(model);
        }
        catch (RateLimitExceededException)
        {
            model.ErrorMessage = "Hit rate limit (5,000 requests an hour)";
            return View(model);
        }
        catch (AuthorizationException)
        {
            model.ErrorMessage = "Not authorized";
            return View(model);
        }
        catch (ArgumentNullException)
        {
            model.ErrorMessage = "Please enter a valid username";
            return View(model);
        }
        catch (Exception e)
        {
            model.ErrorMessage += "Please report this error to kai@devrim.tech";
            model.ErrorMessage = e.ToString();
            return View(model);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}