using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.Tarkov.WebServer.DOTNET.Providers;
using Paulov.Tarkov.WebServer.DOTNET.ResponseModels;

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
        public async void HideoutQTEList(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "qte.json"), out JArray jobj);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(jobj), Request, Response);

        }


        [Route("client/hideout/settings")]
        [HttpPost]
        public async void HideoutSettings(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "settings.json"), out JObject jobj);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(jobj), Request, Response);

        }

        [Route("client/hideout/production/recipes")]
        [HttpPost]
        public async void HideoutProduction(int? retry, bool? debug)
        {
            //DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "production.json"), out JArray jobj);
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "production.json"), out JObject jobj);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(jobj), Request, Response);

        }

        [Route("client/hideout/scavcase")]
        [Route("client/hideout/production/scavcase/recipes")]
        [HttpPost]
        public async void HideoutScavcase(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "scavcase.json"), out JArray jobj);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(jobj), Request, Response);

        }


        [Route("client/hideout/customization/offer/list")]
        [HttpPost]
        public IActionResult HideoutCustomizationList()
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "customisation.json"), out JObject jobj);

            return new BSGSuccessBodyResult(jobj);

        }
    }
}
