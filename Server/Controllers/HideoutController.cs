using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class HideoutController : Controller
    {
        [Route("client/hideout/areas")]
        [HttpPost]
        public IActionResult HideoutAreas()
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "areas.json"), out JArray jobj);

            return new BSGSuccessBodyResult(jobj);

        }


        [Route("client/hideout/qte/list")]
        [HttpPost]
        public async Task<IActionResult> HideoutQTEList(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "qte.json"), out JArray jobj);

            return new BSGSuccessBodyResult(jobj);
        }


        [Route("client/hideout/settings")]
        [HttpPost]
        public async Task<IActionResult> HideoutSettings(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "settings.json"), out JObject jobj);

            return new BSGSuccessBodyResult(jobj);
        }

        [Route("client/hideout/production/recipes")]
        [HttpPost]
        public async Task<IActionResult> HideoutProduction(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "production.json"), out JObject jobj);

            return new BSGSuccessBodyResult(jobj);
        }

        //[Route("client/hideout/scavcase")]
        //[Route("client/hideout/production/scavcase/recipes")]
        //[HttpPost]
        //public async Task<IActionResult> HideoutScavcase(int? retry, bool? debug)
        //{
        //    DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "scavcase.json"), out JArray jobj);

        //    return new BSGSuccessBodyResult(jobj);
        //}


        [Route("client/hideout/customization/offer/list")]
        [HttpPost]
        public IActionResult HideoutCustomizationList()
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "customisation.json"), out JObject jobj);

            return new BSGSuccessBodyResult(jobj);

        }
    }
}
