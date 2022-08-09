namespace StarItAll.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Models;
using Octokit;
using Services;

public class HomeController : Controller
{
    private readonly StarService _star = new();
    private IndexViewModel _model = new();

    [HttpGet("~/")]
    public ActionResult Index()
    {
        return View(_model);
    }

    public ViewResult Index(IndexViewModel model)
    {
        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Index([FromForm] string user)
    {
        _model.Starred = new List<Repository>();
        if (await HttpContext.GetTokenAsync("access_token") == null) Redirect("/sigin");
        var token = await HttpContext.GetTokenAsync("access_token");
        _model = await _star.Star(user, token);
        return View(_model);
    }
}