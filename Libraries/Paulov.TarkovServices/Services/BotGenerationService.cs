using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json.Linq;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Services.Interfaces;
using System.Diagnostics;
using System.Text;
using static EFT.InventoryLogic.Weapon;
using FlatItem = GClass1354;

namespace Paulov.TarkovServices.Services
{
    public class BotGenerationService : IBotGenerationService
    {
        public AccountProfileCharacter BaseBot { get; private set; }

        private Random Randomizer { get; set; } = new Random();

        public IInventoryService InventoryService { get; private set; }

        private JObject _templates;

        public BotGenerationService()
        {
            CreateBaseBot();
            InventoryService = new InventoryService();
        }

        private AccountProfileCharacter CreateBaseBot()
        {
            var stream = FMT.FileTools.EmbeddedResourceHelper.GetEmbeddedResourceByName("scav.json");
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            var str = Encoding.UTF8.GetString(ms.ToArray());
            var scavObj = JObject.Parse(str)["scav"];
            BaseBot = scavObj.ToObject<AccountProfileCharacter>(DatabaseProvider.CachedSerializer);
            return BaseBot;
        }

        public List<AccountProfileCharacter> GenerateBots(List<WaveInfoClass> conditions)
        {
            DatabaseProvider.TryLoadTemplateFile("items.json", out _templates);

            List<AccountProfileCharacter> bots = new List<AccountProfileCharacter>();

            foreach (WaveInfoClass waveInfoClass in conditions)
            {
                for (var i = 0; i < waveInfoClass.Limit; i++)
                {
                    bots.Add(GenerateBot(waveInfoClass));
                }
            }

            // nullify the templates and remove them from memory
            _templates = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

            return bots;
        }

        private AccountProfileCharacter CloneBaseBot()
        {
            return CreateBaseBot();
        }

        public AccountProfileCharacter GenerateBot(WaveInfoClass condition)
        {
            if (condition.Role == WildSpawnType.gifter)
                condition.Role = WildSpawnType.assault;

            var bot = CloneBaseBot();
            var id = MongoID.Generate(false);
            bot.AccountId = Randomizer.Next(1000000, 2000000).ToString();
            bot.Id = id;
            bot.Info.Side = condition.Role == WildSpawnType.pmcBEAR ? EPlayerSide.Bear : condition.Role == WildSpawnType.pmcUSEC ? EPlayerSide.Usec : EPlayerSide.Savage;

            var isPMC = bot.Info.Side != EPlayerSide.Savage;

            bot.Encyclopedia.Clear();
            bot.Hideout = new();
            bot.RagfairInfo = new();
            bot.Stats = new();
            bot.TradersInfo = new();

            // Load the bot database data role (roles are stored in lowercase)
            var lowerRole = condition.Role.ToString().ToLower();
            DatabaseProvider.TryLoadDatabaseFile($"bots/types/{lowerRole}.json", out JObject botDatabaseData);
            if (botDatabaseData == null)
                return bot;

            // Setup Bot Info Settings
            var experienceJson = botDatabaseData["experience"];
            bot.Info.Settings.AggressorBonus = Math.Round(Randomizer.NextDouble(), 2);
            // The Bot difficulty is set from the request condition
            bot.Info.Settings.BotDifficulty = condition.Difficulty;
            // IMPORTANT, UseSimpleAnimator is used for bots like the Zombies
            bot.Info.Settings.UseSimpleAnimator = experienceJson["useSimpleAnimator"].ToString() != "" ? bool.Parse(experienceJson["useSimpleAnimator"].ToString()) : false;
            // The Bot role is set from the request condition
            bot.Info.Settings.Role = condition.Role;
            var experienceRewardJson = botDatabaseData["experience"]["reward"];
            if (experienceRewardJson["normal"] != null)
            {
                bot.Info.Settings.Experience = 275;
            }

            // Generate the bot's Nickname
            var firstnames = botDatabaseData["firstName"].ToArray();
            bot.Info.Nickname = firstnames[Randomizer.Next(firstnames.Length - 1)].ToString();
            if (botDatabaseData.ContainsKey("lastName") && botDatabaseData["lastName"].ToArray().Length > 0)
            {
                var lastnames = botDatabaseData["lastName"].ToArray();
                var lastName = lastnames[Randomizer.Next(lastnames.Length - 1)].ToString();
                if (lastName != "Durkey")
                    bot.Info.Nickname = bot.Info.Nickname + " " + lastName;
            }
            bot.Info.MainProfileNickname = null;
            bot.Info.MemberCategory = EMemberCategory.Default;
            bot.Info.SelectedMemberCategory = EMemberCategory.Default;

            bot.Info.SavageLockTime = 0;

            if (bot.Info.Side != EPlayerSide.Savage)
                bot.Info.TeamId = MongoID.Generate(false);

            if (bot.Info.Side != EPlayerSide.Savage)
                bot.Info.GroupId = MongoID.Generate(false);

            if (bot.Info.Side != EPlayerSide.Savage)
                bot.Info.PrestigeLevel = Randomizer.Next(0, 2);

            GenerateBotLevel(bot);
            AddDogtagToBot(bot);

            if (botDatabaseData.ContainsKey("appearance"))
            {
                // Get the keys to use on the head,body,feet,hands
                var headKeys = ((JObject)botDatabaseData["appearance"]["head"]).Properties().Select(p => p.Name).ToArray();
                var bodyKeys = ((JObject)botDatabaseData["appearance"]["body"]).Properties().Select(p => p.Name).ToArray();
                var feetKeys = ((JObject)botDatabaseData["appearance"]["feet"]).Properties().Select(p => p.Name).ToArray();
                var handKeys = ((JObject)botDatabaseData["appearance"]["hands"]).Properties().Select(p => p.Name).ToArray();

                // Apply a random key to each part
                bot.Customization[EBodyModelPart.Head] = new MongoID(id: headKeys.RandomElement());
                bot.Customization[EBodyModelPart.Body] = new MongoID(id: bodyKeys.RandomElement());
                bot.Customization[EBodyModelPart.Feet] = new MongoID(id: feetKeys.RandomElement());
                bot.Customization[EBodyModelPart.Hands] = new MongoID(id: handKeys.RandomElement());

                // get and apply a random voice key
                var voiceKeys = ((JObject)botDatabaseData["appearance"]["voice"]).Properties().Select(p => p.Name).ToArray();
                bot.Info.Voice = voiceKeys.RandomElement();
            }


            var botDatabaseDataInventory = botDatabaseData["inventory"];
            var botDatabaseDataInventoryEquipment = botDatabaseDataInventory["equipment"];


            InventoryService.RemoveItemFromSlot(bot, "Headwear");

            InventoryService.RemoveItemFromSlot(bot, "FirstPrimaryWeapon");
            var firstprimaryKeys = ((JObject)botDatabaseDataInventoryEquipment["FirstPrimaryWeapon"]).Properties().Select(p => p.Name).ToArray();
            if (firstprimaryKeys.Length > 0)
                AddRandomItemToSlot(bot, "FirstPrimaryWeapon", firstprimaryKeys);

            InventoryService.RemoveItemFromSlot(bot, "SecondPrimaryWeapon");
            var secondprimaryKeys = ((JObject)botDatabaseDataInventoryEquipment["SecondPrimaryWeapon"]).Properties().Select(p => p.Name).ToArray();
            if (secondprimaryKeys.Length > 0)
                AddRandomItemToSlot(bot, "SecondPrimaryWeapon", secondprimaryKeys);

            InventoryService.RemoveItemFromSlot(bot, "Holster");
            var holsterKeys = ((JObject)botDatabaseDataInventoryEquipment["Holster"]).Properties().Select(p => p.Name).ToArray();
            if (holsterKeys.Length > 0)
                AddRandomItemToSlot(bot, "Holster", holsterKeys);

            InventoryService.RemoveItemFromSlot(bot, "pocket1");

            InventoryService.RemoveItemFromSlot(bot, "pocket2");

            InventoryService.RemoveItemFromSlot(bot, "pocket3");

            InventoryService.RemoveItemFromSlot(bot, "pocket4");

            InventoryService.RemoveItemFromSlot(bot, "Armband");

            InventoryService.RemoveItemFromSlot(bot, "Backpack");

            InventoryService.RemoveItemFromSlot(bot, "TacticalVest");

            InventoryService.RemoveItemFromSlot(bot, "Scabbard");
            var scabbardKeys = ((JObject)botDatabaseData["inventory"]["equipment"]["Scabbard"]).Properties().Select(p => p.Name).ToArray();
            if (scabbardKeys.Length > 0)
                AddRandomItemToSlot(bot, "Scabbard", scabbardKeys);

            InventoryService.RemoveItemFromSlot(bot, "ArmorVest");

            // ------------------------------------------------------------------------------------------------
            // The following should be completed AFTER all other actions have been taken on the Bot's Inventory
            //
            //
            //

            // Update the Inventory Equipment Id to something Unique for each bot
            InventoryService.UpdateInventoryEquipmentId(bot);

            // Update each item Id to something Unique
            InventoryService.UpdateMongoIds(bot, InventoryService.GetInventoryItems(bot));

#if DEBUG
            // Display on Debug what we have generated
            Debug.WriteLine($"Generated:{bot.Info.Nickname}:{bot.Id}:{bot.AccountId}:{condition.Role}");
#endif
            return bot;
        }


        private void AddRandomItemToSlot(AccountProfileCharacter bot, string slotId, string[] randomTemplateItems)
        {
            if (randomTemplateItems == null)
                throw new ArgumentNullException(nameof(randomTemplateItems));

            if (randomTemplateItems.Length == 0)
                throw new Exception($"{nameof(randomTemplateItems)}.Length is 0");

            var randomId = randomTemplateItems.RandomElement();
            //var presetItems = DatabaseService.getDatabase().getItemPresetArrayByEncyclopedia(randomId);

            var weaponSlots = new EquipmentSlot[3] { EquipmentSlot.FirstPrimaryWeapon, EquipmentSlot.SecondPrimaryWeapon, EquipmentSlot.Holster };
            var isWeapon = weaponSlots.Contains(Enum.Parse<EquipmentSlot>(slotId));
            // If the item is not a weapon, add it to the inventory with all its mods normally
            if (!isWeapon)
            {
                var newItem = InventoryService.AddTemplatedItemToSlot(bot, randomId, slotId, null);
                InventoryService.AddItemToInventory(bot, newItem);
                //allItems.push(newItem);
                //if (presetItems.length > 0)
                //{
                //    const presetItem = presetItems[this.randomInteger(0, presetItems.length - 1)];
                //    for (let i = 1; i < presetItem._items.length; i++)
                //    {
                //        const item = presetItem._items[i];
                //        if (item)
                //        {
                //            item._id = generateMongoId();
                //            item.parentId = newItem._id;
                //            InventoryService.addItemToInventory(bot, item);
                //            allItems.push(item);
                //        }
                //    }
                //}
                //return newItem;
            }
            else
            {
                // TODO: This needs refactoring in to individual methods

                DatabaseProvider.TryLoadGlobals(out var globals);
                var itemPresets = ((JObject)globals["ItemPresets"]);

                // Assign the weaponItem so we know what ammo to generate for it later on
                FlatItem weaponItem = null;

                List<FlatItem> allAddedItems = new();


                // TODO: This needs to be rewritten to gather a random child that matches the randomId
                foreach (var itempreset in itemPresets.Children())
                {
                    var items = itempreset.Children()["_items"].ToList()[0].ToList();
                    if (items.FindIndex(x => x["_tpl"].ToString() == randomId) != -1)
                    {
                        for (var i = 0; i < items.Count; i++)
                        {
                            var item = items[i].DeepClone().ToObject<GClass1354>();
                            if (i == 0)
                            {
                                item.slotId = slotId;
                                item.parentId = InventoryService.GetEquipmentId(bot);
                                item.upd = new();
                                JObject upd = new JObject();
                                upd["Repairable"] = new JObject()
                                {
                                    { "Durability", bot.Info.Side != EPlayerSide.Savage ? Randomizer.Next(87, 94) : Randomizer.Next(50, 90) },
                                    { "MaxDurability", bot.Info.Side != EPlayerSide.Savage ? Randomizer.Next(95, 99) : Randomizer.Next(91, 99) }
                                };
                                upd["FireMode"] = new JObject()
                                {
                                    { "FireMode", EFireMode.single.ToString() },
                                };
                                item.upd.JToken = upd;
                                weaponItem = item;
                            }

                            allAddedItems.Add(item);
                            InventoryService.AddItemToInventory(bot, item);
                        }
                        // TODO/FIXME: Break. If we don't break here it will attempt to add more than one weapon to the slot
                        break;
                    }
                }

                // Get an ammo type for the weapon and add the ammo to the inventory
                var weaponTemplate = DatabaseProvider.GetTemplateItemById(_templates, weaponItem._tpl);
                var newItems = CreateMagazineWithAmmoForWeapon(weaponItem, allAddedItems.Find(x => x.slotId == "mod_magazine"));
                foreach (var item in newItems)
                    InventoryService.AddItemToInventory(bot, item);

            }

        }

        /// <summary>
        /// Generate ammo for a weapon's magazine. Will also generate the magazine if magazine = null
        /// </summary>
        /// <param name="weaponItem"></param>
        /// <param name="magazine">Will use and fill this magazine if provided</param>
        /// <returns></returns>
        private List<FlatItem> CreateMagazineWithAmmoForWeapon(FlatItem weaponItem, FlatItem magazine = null)
        {
            List<FlatItem> resultItems = new List<FlatItem>();

            var weaponTemplate = DatabaseProvider.GetTemplateItemById(_templates, weaponItem._tpl);
            var weaponTemplateProps = weaponTemplate["_props"];

            if (magazine == null)
                return resultItems;

            var ammoCaliber = weaponTemplate["_props"]?["ammoCaliber"]?.ToString();
            if (string.IsNullOrEmpty(ammoCaliber))
                return resultItems;

            var templatesArray = DatabaseProvider.GetTemplateItemsAsArray(_templates);
            var ammos = templatesArray
                .Where(x => x["_props"]?["ammoType"]?.ToString() == "bullet" && x["_props"]?["Caliber"]?.ToString() == ammoCaliber && float.Parse(x["_props"]?["Damage"]?.ToString()) > 0)
                .OrderBy(x => float.Parse(x["_props"]?["Damage"]?.ToString())).ToList();
            _ = ammos;

            if (ammos.Count == 0)
                return resultItems;

            var magazineTemplate = DatabaseProvider.GetTemplateItemById(_templates, magazine._tpl);
            if (magazineTemplate == null)
                return resultItems;

            if (magazineTemplate["_props"]?["Cartridges"]?.ToArray() != null)
            {
                var selectedRandomAmmo = ammos.RandomElement();
                var magazineMaxCount = int.Parse(magazineTemplate["_props"]["Cartridges"].ToArray()[0]["_max_count"].ToString());

                FlatItem randomAmmo = new()
                {
                    _tpl = selectedRandomAmmo["_id"].ToString(),
                    _id = MongoID.Generate(false),
                    parentId = magazine._id,
                    slotId = "cartridges",
                    upd = new()
                    {
                        JToken = new JObject()
                            {
                                { "StackObjectsCount", magazineMaxCount },
                                { "SpawnedInSession", false },
                            }
                    }
                };
                resultItems.Add(randomAmmo);
            }

            return resultItems;
        }

        private void AddDogtagToBot(AccountProfileCharacter bot)
        {
            if (bot.Info.Side == EPlayerSide.Savage)
                return;

            FlatItem dogtagItem = new FlatItem();
            dogtagItem._id = MongoID.Generate(false);
            dogtagItem._tpl = bot.Info.Side == EPlayerSide.Usec ? "59f32c3b86f77472a31742f0" : "59f32bb586f774757e1e8442";
            dogtagItem.slotId = EquipmentSlot.Dogtag.ToString();
            dogtagItem.parentId = InventoryService.GetEquipmentId(bot);
            dogtagItem.upd = new();
            JObject upd = new JObject();
            upd["Dogtag"] = new JObject()
            {
                { "AccountId", bot.AccountId },
                { "ProfileId", bot.Id.ToString() },
                { "Nickname", bot.Info.Nickname },
                { "Side", bot.Info.Side.ToString() },
                { "Level", bot.Info.Level },
                { "Time", DateTime.UtcNow.ToString() },
                { "Status", "Killed by " },
                { "KillerAccountId", "Unknown" },
                { "KillerProfileId", "Unknown" },
                { "KillerName", "Unknown" },
                { "WeaponName", "Unknown" },
            };
            dogtagItem.upd.JToken = upd;
            InventoryService.AddItemToInventory(bot, dogtagItem);

        }


        private void GenerateBotLevel(AccountProfileCharacter bot)
        {
            if (bot.Info.Side == EPlayerSide.Savage)
                return;

            var level = Randomizer.Next(2, 70);
            var xp = GetNeededXPFromLvl(level);
            bot.Info.Experience = xp;
            bot.Info.Level = level;
        }

        private int GetNeededXPFromLvl(int level)
        {
            var xpTable = Singleton<BackendConfigSettingsClass>.Instance.Experience.Level.Table;

            var exp = 0;
            for (var i = 0; i < level; i++)
            {
                exp += xpTable[i];
            }
            return exp;
        }

        public Dictionary<string, BossLocationSpawn[]> GetZombieSpawners()
        {
            var stream = FMT.FileTools.EmbeddedResourceHelper.GetEmbeddedResourceByName("zombies.json");
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            var str = Encoding.UTF8.GetString(ms.ToArray());
            var zombiesObj = JObject.Parse(str)["halloweenzombies"];

            return zombiesObj.ToObject<Dictionary<string, BossLocationSpawn[]>>(DatabaseProvider.CachedSerializer);
        }

    }
}
