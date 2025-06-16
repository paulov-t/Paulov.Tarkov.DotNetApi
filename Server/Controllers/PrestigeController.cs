using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;

namespace Paulov.Tarkov.Web.Api.Controllers
{
    public class PrestigeController : Controller
    {
        private JsonFileSaveProvider _saveProvider;
        public PrestigeController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("client/prestige/list")]
        [HttpPost]
        public IActionResult List(int? retry, bool? debug)
        {
            return new BSGSuccessBodyResult(DatabaseProvider.GetJsonDocument("templates/prestige.json").RootElement.GetRawText());
        }

        [Route("client/prestige/obtain")]
        [HttpPost]
        public async Task<IActionResult> Obtain(int? retry, bool? debug)
        {
            return new BSGSuccessBodyResult(new { });
        }
    }
}
