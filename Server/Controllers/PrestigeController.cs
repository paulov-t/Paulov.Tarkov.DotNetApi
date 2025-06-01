using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;

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
        public async void List(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG(new { }, Request, Response);
        }

        [Route("client/prestige/obtain")]
        [HttpPost]
        public async void Obtain(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG(new { }, Request, Response);
        }
    }
}
