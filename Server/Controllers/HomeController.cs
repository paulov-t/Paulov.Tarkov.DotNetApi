using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class HomeController : Controller
    {
        IConfiguration configuration;

        public HomeController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [Route("/")]
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.AppVersion = Assembly.GetAssembly(typeof(HomeController)).GetName().Version;
            ViewBag.ServerMode = this.configuration["ServerMode"].ToString();
            return View();
        }

        [Route("/ammo")]
        [HttpGet]
        public IActionResult Ammo()
        {
            ViewBag.AppVersion = Assembly.GetAssembly(typeof(HomeController)).GetName().Version;
            ViewBag.ServerMode = this.configuration["ServerMode"].ToString();
            return View();
        }
    }
}
