using Microsoft.AspNetCore.Mvc;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class QuestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
