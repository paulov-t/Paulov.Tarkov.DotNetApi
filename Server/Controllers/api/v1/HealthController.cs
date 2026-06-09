using Microsoft.AspNetCore.Mvc;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers.api.v1
{
    public class HealthController : Controller
    {
        [Route("/api/v1/Health")]
        [HttpGet]
        public JsonResult Health()
        {
            return Json(new { Status = "OK" });
        }
    }
}
