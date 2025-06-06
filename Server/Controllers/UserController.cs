using Microsoft.AspNetCore.Mvc;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class UserController : Controller
    {
        private readonly SaveProvider _saveProvider;
        public UserController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as SaveProvider;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
