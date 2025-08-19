using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;
using System.Text.Json.Serialization;

namespace Paulov.TarkovServices.Services
{
    public sealed class ActionCommandService : IActionCommandService
    {
        private IAccountService _accountService;
        private IInventoryService _inventoryService;
        private ISaveProvider _saveProvider;

        public ActionCommandService(IAccountService accountService, IInventoryService inventoryService, ISaveProvider saveProvider)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService), "AccountService cannot be null.");
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService), "InventoryService cannot be null.");
            _saveProvider = saveProvider ?? throw new ArgumentNullException(nameof(saveProvider), "SaveProvider cannot be null.");
        }

        public class Result
        {
            public bool Success { get; set; } = true;
            public string? Error { get; set; }
        }

        public class ExecuteCommandResult
        {
            [JsonProperty("profileChanges")]
            [JsonPropertyName("profileChanges")]
            public Dictionary<string, JObject> ProfileChanges { get; set; } = new Dictionary<string, JObject>();

            [JsonProperty("inventoryWarnings")]
            [JsonPropertyName("inventoryWarnings")]
            public JArray InventoryWarnings { get; set; } = new JArray();
        }

        public JObject GenerateEmptyProfileChanges()
        {
            return JObject.FromObject(
            new
            {
                Experience = 0,
                HideoutAreaStashes = new Dictionary<EFT.EAreaType, EFT.HideoutAreaStashInfo>(),
                //Production = new Dictionary<string, EFT.Hideout.ProductionData>(),
                Quests = Array.Empty<RawQuestClass>(),
                RagFairOffers = new EFT.UI.Ragfair.Offer[0],
                RepeatableQuests = Array.Empty<DailyQuestClass>(),
                Stash = new { change = Array.Empty<object>(), del = Array.Empty<object>(), @new = Array.Empty<object>() },
                TradersData = new Dictionary<string, EFT.TraderData>(),
                UnlockedRecipes = new Dictionary<string, bool>()
            });
        }

        public async Task<ExecuteCommandResult> ExecuteCommandAsync(JArray commands, string sessionId)
        {
            // Simulate an asynchronous operation
            await Task.Delay(1); // Simulating some delay for the command execution

            // Create a response object
            ExecuteCommandResult resultData = new ExecuteCommandResult();

            resultData.ProfileChanges.Add(sessionId, GenerateEmptyProfileChanges());

            /**
             * Example of data
             * data: [{ Action: "Move"... }]
             * reload: 16
             * tm: 13
             */
            foreach (var command in commands)
            {
                var commandAction = command["Action"]?.ToString();
                // Process each command here
                // For now, we just log the command
                Console.WriteLine($"Executing command: {commandAction}");
                switch (commandAction)
                {
                    case "Examine":
                        break;
                    case "Eat":
                        break;
                    case "Heal":
                        break;
                    case "HideoutUpgrade":
                        break;
                    case "HideoutUpgradeComplete":
                        break;
                    case "Merge":
                        break;
                    case "Move":
                        ProcessMoveAction(_accountService.GetAccountBySessionId(sessionId), command, resultData);
                        break;
                    case "QuestAccept":
                        break;
                    case "QuestComplete":
                        break;
                    case "RestoreHealth":
                        break;
                    case "Split":
                        break;
                    case "TraderRepair":
                        break;
                    case "TradingConfirm":
                        break;
                    case "Transfer":
                        break;
                    case "QuestHandover":
                        break;
                    // Add more cases for different commands as needed
                    default:
                        Console.WriteLine($"Unknown command: {commandAction}");
                        break;
                }
            }

            return resultData;
        }

        public Result ProcessMoveAction(Account account, JToken action, ExecuteCommandResult outputChanges)
        {
            if (!outputChanges.ProfileChanges.ContainsKey(account.AccountId))
            {
                outputChanges.ProfileChanges.Add(account.AccountId, GenerateEmptyProfileChanges());
            }

            outputChanges.ProfileChanges[account.AccountId]["Items"] = new JObject();


            var result = new Result();

            var accountProfile = _saveProvider.GetAccountProfileMode(account);
            var inventoryItems = _inventoryService.GetInventoryItems(accountProfile.Characters.PMC);

            var matchingInventoryItem = inventoryItems.FirstOrDefault(item => item._id == action["item"].ToString());

            if (matchingInventoryItem != null)
            {
                var jToken = JToken.Parse(action["to"]["location"].ToString());
                matchingInventoryItem.location = new UnparsedData() { JToken = jToken };
                if (action["to"]["id"] != null)
                    matchingInventoryItem.parentId = new EFT.MongoID(action["to"]["id"].ToString());
                else if (action["to"]["container"] != null && !string.IsNullOrEmpty(action["to"]["container"].ToString()))
                {
                    if (action["to"]["container"].ToString().StartsWith("pocket"))
                    {
                        matchingInventoryItem.parentId = inventoryItems.FirstOrDefault(item => item.slotId == "Pockets")._id;
                    }
                }
                // Moving to container hideout. Use StashId
                else if (action["to"]["container"] != null && action["to"]["container"].ToString() == "hideout")
                    matchingInventoryItem.parentId = _inventoryService.GetStashId(accountProfile.Characters.PMC);
                matchingInventoryItem.slotId = action["to"]["container"] != null ? action["to"]["container"].ToString() : null;

            }

            _inventoryService.SetInventoryItems(accountProfile.Characters.PMC, inventoryItems);
            _saveProvider.SaveProfile(account.AccountId, account);

            return result;
        }
    }
}
