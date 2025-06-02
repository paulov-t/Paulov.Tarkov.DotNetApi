using Microsoft.AspNetCore.Mvc;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
