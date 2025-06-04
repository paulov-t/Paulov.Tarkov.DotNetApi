using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Paulov.TarkovServices;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers.api.v1
{
    public class ItemSearchController : Controller
    {
        [Route("/itemSearch/getAmmo/")]
        [HttpPost]
        public IActionResult Index()
        {
            DatabaseProvider.TryLoadDatabaseFile("locales/global/en.json", out Dictionary<string, dynamic> languageLocaleData);
            DatabaseProvider.TryLoadDatabaseFile("templates/items.json", out Dictionary<string, dynamic> templatesItemData);

            const string ammoParentId = "5485a8684bdc2da71d8b4567";
            string[] ammoIdsToIgnore = { "5996f6d686f77467977ba6cc", "5d2f2ab648f03550091993ca", "5cde8864d7f00c0010373be1" };

            List<dynamic> result = new List<dynamic>();
            foreach (var itemId in templatesItemData.Keys)
            {
                var item = templatesItemData[itemId];
                var name = item._name;

                if (item._parent != "5485a8684bdc2da71d8b4567")
                    continue;

                if (ammoIdsToIgnore.IndexOf(itemId) != -1)
                    continue;

                if (item._props.ammoType != "bullet")
                    continue;

                var langItem = languageLocaleData.ContainsKey($"{itemId} Name") ? languageLocaleData[$"{itemId} Name"] : null;
                if (langItem == null)
                    langItem = languageLocaleData.ContainsKey($"{itemId} ShortName") ? languageLocaleData[$"{itemId} ShortName"] : null;

                if (langItem == null)
                    langItem = item._name;

                var parentIdLang = languageLocaleData.ContainsKey($"{item._parent} Name") ? languageLocaleData[$"{item._parent} Name"] : null;
                if (parentIdLang == null)
                    parentIdLang = "N/A";

                var rating = 0;// Math.ceil((item.PaulovRating / highestRating) * 100);

                result.Add(new
                {
                    itemId = itemId,
                    langItem = langItem,
                    caliber = item._props.Caliber,
                    armorDamage = item._props.ArmorDamage,
                    penetration = item._props.PenetrationPower,
                    damage = item._props.Damage,
                    rating = rating
                });
            }

            return new BSGSuccessBodyResult(result);
        }
    }
}
