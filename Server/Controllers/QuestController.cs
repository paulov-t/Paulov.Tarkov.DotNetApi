using Microsoft.AspNetCore.Mvc;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class QuestController : Controller
    {
        private JsonFileSaveProvider _saveProvider;
        public QuestController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
