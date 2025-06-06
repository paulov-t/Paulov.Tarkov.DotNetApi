using Microsoft.AspNetCore.Mvc;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class QuestController : Controller
    {
        private SaveProvider _saveProvider;
        public QuestController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as SaveProvider;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
