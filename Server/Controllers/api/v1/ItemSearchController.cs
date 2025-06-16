using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using BSGHelperLibrary.ResponseModels;
using EFT.InventoryLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
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
            using FormReader formReader = new FormReader(Request.Body);
            Dictionary<string, StringValues> decodedForm = await formReader.ReadFormAsync();

            int start = int.Parse(decodedForm["start"].First());
            int length = int.MaxValue;
            if (decodedForm.TryGetValue("length", out  StringValues lengthValues))
            {
                length = int.Parse(lengthValues.First());
            }
            
            //Localization pre-loading
            IEnumerable<KeyValuePair<string, JToken>> enumerableLocalizations =
                DatabaseProvider.LoadDatabaseFileAsEnumerable("locales/global/en.json");
            Dictionary<string, string> localizationDictionary =
                enumerableLocalizations.ToDictionary(localization =>
                    localization.Key, localization => localization.Value.ToString());

            //Price pre-loading
            IEnumerable<KeyValuePair<string, JToken>> enumerablePrices =
                DatabaseProvider.LoadDatabaseFileAsEnumerable("templates/prices.json");
            Dictionary<string, long> pricesDictionary =
                enumerablePrices.ToDictionary(price =>
                    price.Key, price => long.Parse(price.Value.ToString()));
            
            //Item pre-loading
            IEnumerable<KeyValuePair<string, JToken>> enumerableItems =
                DatabaseProvider.LoadDatabaseFileAsEnumerable("templates/items.json");
            List<MinimalTemplateItem> items = enumerableItems.Select(x =>
                new MinimalTemplateItem(x.Value, pricesDictionary.GetValueOrDefault(x.Key))).ToList();
            
            //Highest price
            long highestPrice = items.Max(x => x.Price);

            JArray rootResponseObject = [];
            int index = 0;
            foreach (MinimalTemplateItem item in items)
            {
                if (index++ < start) continue; //NOTE: I think needing this is a bug on the web side?
                //Localized name
                string localizedItemName = item.ItemID;
                if (!localizationDictionary.TryGetValue($"{item.ItemID} Name", out localizedItemName))
                {
                    _ = localizationDictionary.TryGetValue($"{item.ItemID} ShortName", out localizedItemName);
                }

                //Localized parent name
                string localizedParentItemName = "N/A";
                if (!string.IsNullOrEmpty(item.ParentID))
                {
                    _ = localizationDictionary.TryGetValue($"{item.ParentID} Name", out localizedParentItemName);
                }

                int rating = 0;
                double priceRatio = 0;
                if (item.Price > 0)
                {
                    if (string.IsNullOrEmpty(item.PvERarity)) continue;
                    double initialRatio = Math.Round((double)item.Price / highestPrice * 100);
                    priceRatio = initialRatio * 3;
                    const double basePvERarityMultiplier = 2.5;

                    Enum.TryParse(item.PvERarity, true, out PvERarity rarity);
                    
                    priceRatio *= basePvERarityMultiplier + ((int)rarity * 0.25);
                    priceRatio = Math.Max(Math.Min(priceRatio, 100), 1);
                    priceRatio = Math.Ceiling(priceRatio);
                }

                if (rootResponseObject.Count > length) break;
                
                rootResponseObject.Add(new JObject
                {
                    ["itemId"] = item.ItemID,
                    ["langItem"] = localizedItemName,
                    ["rating"] = rating,
                    ["parentId"] = item.ParentID,
                    ["parentIdLang"] = localizedParentItemName,
                    ["price"] = item.Price,
                    ["priceRatio"] = priceRatio
                });
            }
            
            return new BSGSuccessBodyResult(rootResponseObject);
        }


        [Route("/itemSearch/getAmmo/")]
        [HttpPost]
        public async Task<IActionResult> Ammo()
        {
            //Localization pre-loading
            IEnumerable<KeyValuePair<string, JToken>> enumerableLocalizations =
                DatabaseProvider.LoadDatabaseFileAsEnumerable("locales/global/en.json");
            ConcurrentDictionary<string, string> localizationDictionary =
                new(enumerableLocalizations.Select(localization =>
                    new KeyValuePair<string, string>(localization.Key, localization.Value.ToString())));
            
            
            //Item pre-loading
            IEnumerable<MinimalTemplateItem> templatesItemsMinimalEnumerable =
                DatabaseProvider.LoadDatabaseFileAsEnumerable("templates/items.json")
                    .Select(x => new MinimalTemplateItem(x.Value, 0));
            
            const string ammoParentId = "5485a8684bdc2da71d8b4567";
            string[] ammoIdsToIgnore = ["5996f6d686f77467977ba6cc", "5d2f2ab648f03550091993ca", "5cde8864d7f00c0010373be1"];

            JArray rootResponseObject = [];
            await Parallel.ForEachAsync(templatesItemsMinimalEnumerable, (item, _) =>
            {
                if (!string.Equals(item.ParentID, ammoParentId)) return ValueTask.CompletedTask;
                if (ammoIdsToIgnore.Contains(item.ItemID)) return ValueTask.CompletedTask;
                if (!string.Equals(item.Props.AmmoType, "bullet")) return ValueTask.CompletedTask;
                
                //Localized name
                string localizedItemName = item.ItemID;
                if (!localizationDictionary.TryGetValue($"{item.ItemID} Name", out localizedItemName))
                {
                    localizationDictionary.TryGetValue($"{item.ItemID} ShortName", out localizedItemName);
                }

                int rating = 0; // Math.ceil((item.PaulovRating / highestRating) * 100);
                
                rootResponseObject.Add(new JObject
                {
                    ["itemId"] = item.ItemID,
                    ["langItem"] = localizedItemName,
                    ["caliber"] = item.Props.Caliber,
                    ["armorDamage"] = item.Props.ArmorDamage,
                    ["penetration"] = item.Props.PenetrationPower,
                    ["damage"] = item.Props.Damage,
                    ["rating"] = rating
                });
                return ValueTask.CompletedTask;
            });
            return new BSGSuccessBodyResult(rootResponseObject);
        }

        //TODO: Look into sharing references to shared strings such as caliber and parent ID
        private readonly struct MinimalTemplateItem(JToken templateItem, long price)
        {
            public readonly string ItemID = templateItem["_id"]?.ToString() ?? string.Empty;
            public readonly string ParentID = templateItem["_parent"]?.ToString() ?? string.Empty;
            public readonly string PvERarity = templateItem.SelectToken("_props.RarityPvE")?.ToString() ?? string.Empty;
            public readonly MinimalTemplateItemProps Props = new(templateItem["_props"]);
            public readonly long Price = price;
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
