namespace StarItAll.Controllers;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    [HttpGet("~/")]
    public ActionResult Index() => View();
}

