using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class LocationController : ControllerBase
    {
        /// <summary>
        /// Provides an object of locations (base) and paths between each location
        /// </summary>
        /// <returns></returns>
        [Route("client/locations")]
        [HttpPost]
        public async Task<IActionResult> Locations()
        {
            if (!DatabaseProvider.TryLoadLocationBases(out var locationJsons))
            {
                Response.StatusCode = 500;
                return new BSGResult(null);
            }

            if (!DatabaseProvider.TryLoadLocationPaths(out var paths))
            {
                Response.StatusCode = 500;
                return new BSGResult(null);
            }

            JObject result = new JObject();
            result.Add("locations", locationJsons);
            result.Add("paths", paths);

            return new BSGSuccessBodyResult(result);

        }
    }
}
