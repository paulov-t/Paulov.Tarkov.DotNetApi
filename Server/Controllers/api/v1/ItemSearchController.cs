using System.Collections;
using System.Diagnostics;
using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices;
using Sirenix.Serialization;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers.api.v1
{
    public class ItemSearchController : Controller
    {
        [Route("/itemSearch/getItemEnglishNameAndTpl/")]
        [HttpPost]
        public async Task<IActionResult> Items()
        {
            Dictionary<string, object> requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            int start = 0;
            int length = int.MaxValue;

            if (requestBody.TryGetValue("start", out object startValue))
            {
                start = int.Parse(startValue.ToString() ?? "0");
                length = int.Parse(requestBody["length"].ToString() ?? int.MaxValue.ToString());
            }

            DatabaseProvider.TryLoadDatabaseFile("locales/global/en.json", out Dictionary<string, dynamic> languageLocaleData);
            DatabaseProvider.TryLoadDatabaseFile("templates/prices.json", out Dictionary<string, dynamic> templatesPricesData);
            IEnumerable<KeyValuePair<string, JObject>> templatesItemEnumerable =
                DatabaseProvider.LoadDatabaseFileAsEnumerable("templates/items.json");

            List<MinimalTemplateItem> minimalItems =
                templatesItemEnumerable.Select(x => new MinimalTemplateItem(x.Value)).ToList();
            int highestPrice = minimalItems.Select(x =>
            {
                if(templatesPricesData.TryGetValue(x.ItemID, out dynamic price)) return (int)price;
                return 0;
            }).Max();

            const string ammoParentId = "5485a8684bdc2da71d8b4567";
            string[] ammoIdsToIgnore = { "5996f6d686f77467977ba6cc", "5d2f2ab648f03550091993ca", "5cde8864d7f00c0010373be1" };
            
            List<dynamic> results = new List<dynamic>();

            foreach (MinimalTemplateItem item in minimalItems)
            {
                //Localized name
                string localizedItemName = item.ItemID;
                if (languageLocaleData.TryGetValue($"{item.ItemID} Name", out var itemLongName))
                {
                    localizedItemName = (string)itemLongName;
                }
                else
                {
                    if (languageLocaleData.TryGetValue($"{item.ItemID} ShortName", out var itemShortName))
                    {
                        localizedItemName = (string)itemShortName;
                    }
                }

                //Localized parent name
                string localizedParentItemName = "N/A";
                if (!string.IsNullOrEmpty(item.ParentID) &&
                    languageLocaleData.TryGetValue($"{item.ParentID} Name", out var parentLongName))
                {
                    localizedParentItemName = (string)parentLongName;
                }
                
                //Ratings
                int rating = 0;
                int price = (int)(templatesPricesData.GetValueOrDefault(item.ItemID) ?? 0);
                double priceRatio = 0;
                if (price > 0)
                {
                    if (string.IsNullOrEmpty(item.PvERarity)) continue;
                    
                    double initialRatio = Math.Round((double)price / highestPrice * 100);
                    priceRatio = initialRatio * 3;
                    const double basePvERarityMultiplier = 2.5;

                    Enum.TryParse(item.PvERarity, true, out PvERarity rarity);
                    
                    priceRatio *= basePvERarityMultiplier + ((int)rarity * 0.25);
                    priceRatio = Math.Max(Math.Min(priceRatio, 100), 1);
                    priceRatio = Math.Ceiling(priceRatio);
                }

                if (results.Count > length * (start + 1)) break;
                
                results.Add(new
                {
                    itemId = item.ItemID,
                    langItem = (dynamic)localizedItemName,
                    rating = rating,
                    parentId = (dynamic)item.ParentID,
                    parentIdLang = (dynamic)localizedParentItemName,
                    price = price,
                    priceRatio = priceRatio
                });
            }
            
            return new BSGSuccessBodyResult(results);
        }


        [Route("/itemSearch/getAmmo/")]
        [HttpPost]
        public async Task<IActionResult> Ammo()
        {
            DatabaseProvider.TryLoadDatabaseFile("locales/global/en.json", out Dictionary<string, dynamic> languageLocaleData);
            //Loading in as a lazy enumerable means we can convert to a smaller object on the fly without a large allocation
            IEnumerable<MinimalTemplateItem> templatesItemsMinimalEnumerable =
                DatabaseProvider.LoadDatabaseFileAsEnumerable("templates/items.json")
                    .Select(x => new MinimalTemplateItem(x.Value));
            const string ammoParentId = "5485a8684bdc2da71d8b4567";
            string[] ammoIdsToIgnore = ["5996f6d686f77467977ba6cc", "5d2f2ab648f03550091993ca", "5cde8864d7f00c0010373be1"];
            
            List<dynamic> result = new List<dynamic>();
            await Parallel.ForEachAsync(templatesItemsMinimalEnumerable, (item, _) =>
            {
                if (!string.Equals(item.ParentID, ammoParentId)) return ValueTask.CompletedTask;
                if (ammoIdsToIgnore.Contains(item.ItemID)) return ValueTask.CompletedTask;
                if (!string.Equals(item.Props.AmmoType, "bullet")) return ValueTask.CompletedTask;
                
                //Localized name
                string localizedItemName = item.ItemID;
                if (languageLocaleData.TryGetValue($"{item.ItemID} Name", out var itemLongName))
                {
                    localizedItemName = (string)itemLongName;
                }
                else
                {
                    if (languageLocaleData.TryGetValue($"{item.ItemID} ShortName", out var itemShortName))
                    {
                        localizedItemName = (string)itemShortName;
                    }
                }

                int rating = 0; // Math.ceil((item.PaulovRating / highestRating) * 100);
                
                result.Add(new
                {
                    itemId = item.ItemID,
                    langItem = localizedItemName,
                    caliber = item.Props.Caliber,
                    armorDamage = item.Props.ArmorDamage,
                    penetration = item.Props.PenetrationPower,
                    damage = item.Props.Damage,
                    rating = rating
                });
                return ValueTask.CompletedTask;
            });
            return new BSGSuccessBodyResult(result);
        }

        private readonly struct MinimalTemplateItem(JObject templateItem)
        {
            public readonly string ItemID = templateItem.GetValue("_id")?.ToString() ?? string.Empty;
            public readonly string ParentID = templateItem.GetValue("_parent")?.ToString() ?? string.Empty;
            public readonly string PvERarity = templateItem.SelectToken("_props.RarityPvE")?.ToString() ?? string.Empty;
            public readonly MinimalTemplateItemProps Props = new(templateItem.GetValue("_props"));
        }

        private readonly struct MinimalTemplateItemProps(JToken templateItemProps)
        {
            public readonly string Caliber = templateItemProps["Caliber"]?.ToString() ?? string.Empty;
            public readonly string AmmoType = templateItemProps["ammoType"]?.ToString() ?? string.Empty;
            public readonly int ArmorDamage = (int)(templateItemProps["ArmorDamage"] ?? 0);
            public readonly int PenetrationPower = (int)(templateItemProps["PenetrationPower"] ?? 0);
            public readonly int Damage = (int)(templateItemProps["Damage"] ?? 0);
        }
    }

    enum PvERarity
    {
        Unknown = 0,
        Common = 0,
        Rare = 1,
        Superrare = 2,
        Not_exist = 2,
    }
}
