using Microsoft.AspNetCore.Mvc;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class HideoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
