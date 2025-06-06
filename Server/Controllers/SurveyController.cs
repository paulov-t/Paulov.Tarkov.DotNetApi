using Microsoft.AspNetCore.Mvc;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class SurveyController : Controller
    {
        private SaveProvider _saveProvider;
        public SurveyController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as SaveProvider;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
