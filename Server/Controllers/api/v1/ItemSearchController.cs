using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers.api.v1
{
    public class ItemSearchController : Controller
    {
        [Route("/itemSearch/getItemEnglishNameAndTpl/")]
        [HttpPost]
        public async Task<IActionResult> Items()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            var start = requestBody.ContainsKey("start") ? int.Parse(requestBody["start"].ToString()) : 0;
            var length = requestBody.ContainsKey("start") ? int.Parse(requestBody["length"].ToString()) : int.MaxValue;

            DatabaseProvider.TryLoadDatabaseFile("locales/global/en.json", out Dictionary<string, dynamic> languageLocaleData);
            DatabaseProvider.TryLoadDatabaseFile("templates/items.json", out Dictionary<string, dynamic> templatesItemData);
            DatabaseProvider.TryLoadDatabaseFile("templates/prices.json", out Dictionary<string, dynamic> templatesPricesData);

            int highestPrice = 0;
            foreach (var itemId in templatesPricesData.Keys)
            {
                var item = templatesItemData[itemId];

                if (templatesPricesData[itemId] > highestPrice)
                    highestPrice = (int)templatesPricesData[itemId];
            }

            const string ammoParentId = "5485a8684bdc2da71d8b4567";
            string[] ammoIdsToIgnore = { "5996f6d686f77467977ba6cc", "5d2f2ab648f03550091993ca", "5cde8864d7f00c0010373be1" };

            var index = 0;
            List<dynamic> result = new List<dynamic>();
            foreach (var itemId in templatesItemData.Keys)
            {
                var item = templatesItemData[itemId];
                var name = item._name;

                var langItem = languageLocaleData.ContainsKey($"{itemId} Name") ? languageLocaleData[$"{itemId} Name"] : null;
                if (langItem == null)
                    langItem = languageLocaleData.ContainsKey($"{itemId} ShortName") ? languageLocaleData[$"{itemId} ShortName"] : null;

                if (langItem == null)
                    langItem = item._name;

                var parentIdLang = languageLocaleData.ContainsKey($"{item._parent} Name") ? languageLocaleData[$"{item._parent} Name"] : null;
                if (parentIdLang == null)
                    parentIdLang = "N/A";

                var rating = 0;// Math.ceil((item.PaulovRating / highestRating) * 100);
                var price = DatabaseProvider.GetTemplateItemPrice(itemId);
                double priceRatio = 0;
                if (price > 0)
                {
                    var initialRatio = ((double)price / (double)highestPrice) * 100;
                    priceRatio = Math.Round(initialRatio);
                    priceRatio *= 3;
                    if (item._props == null)
                        continue;

                    if (item._props.RarityPvE == null)
                        continue;

                    switch (item._props.RarityPvE)
                    {
                        case "Superrare":
                            priceRatio *= 3;
                            break;
                        case "Rare":
                            priceRatio *= 2.75;
                            break;
                        // Common
                        case "Common":
                            priceRatio *= 2.5;
                            break;
                        // Not exist is loot table specific
                        case "Not_exist":
                            priceRatio *= 3;
                            break;
                        default:
                            priceRatio *= 2.5;
                            break;
                    }
                    priceRatio = Math.Min(priceRatio, 100);
                    priceRatio = Math.Max(priceRatio, 1);
                    priceRatio = Math.Ceiling((double)priceRatio);
                }

                index++;
                if (index > length * (start + 1))
                    break;

                result.Add(new
                {
                    itemId = itemId,
                    langItem = langItem,
                    rating = rating,
                    parentId = item._parent,
                    parentIdLang = parentIdLang,
                    price = price,
                    priceRatio = priceRatio
                });
            }

            return new BSGSuccessBodyResult(result);
        }


        [Route("/itemSearch/getAmmo/")]
        [HttpPost]
        public IActionResult Ammo()
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
