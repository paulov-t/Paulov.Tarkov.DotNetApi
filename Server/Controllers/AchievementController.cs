using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class AchievementController : Controller
    {
        private JsonFileSaveProvider _saveProvider;
        public AchievementController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
        }

        [Route("client/achievement/statistic", Name = "AchievementStat")]
        [HttpPost]
        public IActionResult AchievementStat(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile("templates/achievements.json", out JArray dbFile);

            JObject dbObject = new JObject();
            dbObject.Add("elements", new JObject());
            return new BSGSuccessBodyResult(dbObject);
        }

        [Route("client/achievement/list", Name = "AchievementList")]
        [HttpPost]
        public IActionResult AchievementList(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile("templates/achievements.json", out JArray dbFile);

            JObject dbObject = new JObject();
            dbObject.Add("elements", dbFile);
            return new BSGSuccessBodyResult(dbObject);
        }
    }
}
