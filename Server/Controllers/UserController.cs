using Microsoft.AspNetCore.Mvc;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class UserController : Controller
    {
        private readonly JsonFileSaveProvider _saveProvider;
        public UserController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
