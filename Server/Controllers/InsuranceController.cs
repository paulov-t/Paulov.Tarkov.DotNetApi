using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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

            var requestedTraders = requestBody["traders"] as JArray;

            JObject result = new JObject();
            foreach (var t in requestedTraders)
            {
                result.Add(t.ToString(), new JObject());
            }

            return new BSGSuccessBodyResult(result);
        }
    }
}
