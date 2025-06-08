using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class InsuranceController : Controller
    {
        [Route("client/insurance/items/list/cost")]
        [HttpPost]
        public async Task<IActionResult> InsuranceItemsListCost()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            if (requestBody == null)
                return new BSGErrorBodyResult(500, "");


            return new BSGSuccessBodyResult(new { });
        }
    }
}
