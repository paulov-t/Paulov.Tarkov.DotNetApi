using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Paulov.TarkovServices;

namespace Paulov.Tarkov.Web.Api.Controllers
{
    public class PrestigeController : Controller
    {
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
