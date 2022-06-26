namespace StarItAll.Controllers;

using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using Models;
using Octokit;
using Activity = System.Diagnostics.Activity;

public class HomeController : Controller
{
    private readonly GitHubClient _client = new(new ProductHeaderValue("StarItAllWeb"));
    private readonly IConfiguration _config;
    private readonly ILogger<HomeController> _logger;
    private readonly IndexViewModel _model = new();

    public HomeController(ILogger<HomeController> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public IActionResult Index()
    {
        var accessToken = HttpContext.Session.GetString("OAuthToken");
        if (accessToken != null) _client.Credentials = new Credentials(accessToken);
        try
        {
            _model.ErrorMessage = string.Empty;
            _model.Starred = new List<Repository>();
            return View(_model);
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
            if (Environment.GetEnvironmentVariable("ClientId") != null &&
                Environment.GetEnvironmentVariable("ClientSecret") != null &&
                Environment.GetEnvironmentVariable("CSRF") != null)
            {
                var token = await _client.Oauth.CreateAccessToken(
                    new OauthTokenRequest(Environment.GetEnvironmentVariable("ClientId"),
                        Environment.GetEnvironmentVariable("ClientSecret"), code));
                HttpContext.Session.SetString("OAuthToken", token.AccessToken);
            }
            else if (_config["ClientId"] != null && _config["ClientSecret"] != null && _config["CSRF"] != null)
            {
                var token = await _client.Oauth.CreateAccessToken(
                    new OauthTokenRequest(_config["ClientId"], _config["ClientSecret"], code));
                HttpContext.Session.SetString("OAuthToken", token.AccessToken);
            }
            else
            {
                throw new AuthenticationException(
                    "Did not set the ClientId or Client in your config or environment variables.");
            }
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
        _model.Starred = new List<Repository>();
        try
        {
            var repos = await _client.Repository.GetAllForUser(user);
            foreach (var repo in repos)
            {
                await _client.Activity.Starring.StarRepo(user, repo.Name);
                _model.Starred.Add(repo);
            }

            _model.Username = user;
            _model.ErrorMessage = string.Empty;
            return View(_model);
        }
        catch (NotFoundException)
        {
            _model.ErrorMessage = "User not found";
            return View(_model);
        }
        catch (RateLimitExceededException)
        {
            _model.ErrorMessage = "Hit rate limit (5,000 requests an hour)";
            return View(_model);
        }
        catch (AuthorizationException)
        {
            _model.ErrorMessage = "Not authorized";
            return View(_model);
        }
        catch (ArgumentNullException)
        {
            _model.ErrorMessage = "Please enter a valid username";
            return View(_model);
        }
        catch (Exception e)
        {
            _model.ErrorMessage += "Please report this error to kai@devrim.tech";
            _model.ErrorMessage = e.ToString();
            return View(_model);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}