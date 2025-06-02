using Microsoft.AspNetCore.Mvc;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class SurveyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
