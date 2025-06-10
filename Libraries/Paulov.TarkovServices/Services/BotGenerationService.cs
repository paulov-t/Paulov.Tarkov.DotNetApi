using EFT;
using Newtonsoft.Json.Linq;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Services.Interfaces;
using System.Diagnostics;
using System.Text;

namespace Paulov.TarkovServices.Services
{
    public class BotGenerationService : IBotGenerationService
    {
        public AccountProfileCharacter BaseBot { get; private set; }

        private Random Randomizer { get; set; } = new Random();

        public IInventoryService InventoryService { get; private set; }

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
            List<AccountProfileCharacter> bots = new List<AccountProfileCharacter>();

            foreach (WaveInfoClass waveInfoClass in conditions)
            {
                for (var i = 0; i < waveInfoClass.Limit; i++)
                {
                    bots.Add(GenerateBot(waveInfoClass));
                }
            }

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

            var lowerRole = condition.Role.ToString().ToLower();
            DatabaseProvider.TryLoadDatabaseFile($"bots/types/{lowerRole}.json", out JObject botDatabaseData);
            if (botDatabaseData == null)
                return bot;

            var experienceJson = botDatabaseData["experience"];
            bot.Info.Settings.BotDifficulty = condition.Difficulty;
            bot.Info.Settings.UseSimpleAnimator = experienceJson["useSimpleAnimator"].ToString() != "" ? bool.Parse(experienceJson["useSimpleAnimator"].ToString()) : false;
            bot.Info.Settings.Role = condition.Role;

            // Generate the bot's Nickname
            var firstnames = botDatabaseData["firstName"].ToArray();
            bot.Info.Nickname = botDatabaseData["firstName"].ToArray()[Randomizer.Next(firstnames.Length - 1)].ToString();
            if (botDatabaseData.ContainsKey("lastName") && botDatabaseData["lastName"].ToArray().Length > 0)
            {
                var lastnames = botDatabaseData["lastName"].ToArray();
                var lastName = lastnames[Randomizer.Next(lastnames.Length - 1)].ToString();
                if (lastName != "Durkey")
                    bot.Info.Nickname = bot.Info.Nickname + " " + lastName;
            }
            bot.Info.MainProfileNickname = null;
            bot.Info.SelectedMemberCategory = EMemberCategory.Default;

            if (botDatabaseData.ContainsKey("appearance"))
            {
                var headKeys = ((JObject)botDatabaseData["appearance"]["head"]).Properties().Select(p => p.Name).ToArray();
                var bodyKeys = ((JObject)botDatabaseData["appearance"]["body"]).Properties().Select(p => p.Name).ToArray();
                var feetKeys = ((JObject)botDatabaseData["appearance"]["feet"]).Properties().Select(p => p.Name).ToArray();
                var handKeys = ((JObject)botDatabaseData["appearance"]["hands"]).Properties().Select(p => p.Name).ToArray();

                bot.Customization[EBodyModelPart.Head] = new MongoID(id: headKeys.RandomElement());
                bot.Customization[EBodyModelPart.Body] = new MongoID(id: bodyKeys.RandomElement());
                bot.Customization[EBodyModelPart.Feet] = new MongoID(id: feetKeys.RandomElement());
                bot.Customization[EBodyModelPart.Hands] = new MongoID(id: handKeys.RandomElement());

                var voiceKeys = ((JObject)botDatabaseData["appearance"]["voice"]).Properties().Select(p => p.Name).ToArray();
                bot.Info.Voice = voiceKeys.RandomElement();
            }


            InventoryService.UpdateInventoryEquipmentId(bot);
            InventoryService.UpdateMongoIds(bot);

            Debug.WriteLine($"Generated:{bot.Info.Nickname}:{bot.Id}:{bot.AccountId}:{condition.Role}");
            return bot;
        }


    }
}
