using Microsoft.AspNetCore.Mvc;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class FriendController : Controller
    {
        private SaveProvider _saveProvider;
        public FriendController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as SaveProvider;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
