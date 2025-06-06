using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers.api.v1
{
    public class UserController : Controller
    {
        [Route("/v1/user/live")]
        [HttpGet]
        public IActionResult LivePlayers()
        {
            return new BSGSuccessBodyResult(new { });
        }
    }
}
