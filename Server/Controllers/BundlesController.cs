using Microsoft.AspNetCore.Mvc;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class BundlesController : ControllerBase
    {
        [Route("singleplayer/bundles")]
        [HttpGet]
        public IActionResult Bundles(int? retry, bool? debug)
        {
            // This is a custom call. Use JsonResult
            return new JsonResult(Array.Empty<string>());
        }
    }
}
