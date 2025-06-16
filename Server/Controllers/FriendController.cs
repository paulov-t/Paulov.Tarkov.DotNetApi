using Microsoft.AspNetCore.Mvc;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class FriendController : Controller
    {
        private JsonFileSaveProvider _saveProvider;
        public FriendController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
