namespace StarItAll.Controllers;

using Microsoft.AspNetCore.Mvc;
using Models;
using Octokit;

public class HomeController : Controller
{
    private readonly GitHubClient _client = new(new ProductHeaderValue("StarItAllWeb"));
    private readonly IndexViewModel _model = new();
    
    [HttpGet("~/")]
    public ActionResult Index()
    {
        return View(_model);
    }

    [HttpPost]
    public async Task<ActionResult> Index([FromForm] string user)
    {
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
}