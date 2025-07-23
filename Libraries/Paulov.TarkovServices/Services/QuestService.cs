using Comfort.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Helpers;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Paulov.TarkovServices.Services
{
    public sealed class QuestService : IQuestService
    {
        private IDatabaseProvider _dbProvider;
        private ISaveProvider _saveProvider;

        // This service is currently empty, but it can be expanded in the future to handle quest-related logic.
        // For example, it could manage quest states, progress, and interactions with other services.

        public QuestService(IDatabaseProvider dbProvider, ISaveProvider saveProvider)
        {
            // Constructor logic can be added here if needed in the future.
            ArgumentNullException.ThrowIfNull(nameof(dbProvider), "Database provider cannot be null.");
            ArgumentNullException.ThrowIfNull(nameof(saveProvider), "Save provider cannot be null.");
            _dbProvider = dbProvider;
            _saveProvider = saveProvider;
        }



        public List<RawQuestClass> GetQuestsForAccount(Account account)
        {
            ArgumentNullException.ThrowIfNull(nameof(account), "Account cannot be null.");

            if (_saveProvider.GetPmcProfile(account).QuestsData == null)
            {
                _saveProvider.GetPmcProfile(account).QuestsData = new List<QuestDataClass>();
            }

            var entryStream = _dbProvider.GetEntryStream("database/templates/quests.json");
            if (entryStream == null)
            {
                throw new FileNotFoundException("Quests data file not found in the database.");
            }

            using var reader = new StreamReader(entryStream, Encoding.UTF8);
            var jsonContent = reader.ReadToEnd();


            var jsonDocumentText = JsonDocument.Parse(jsonContent).RootElement.GetRawText();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                }
                ,
                AllowTrailingCommas = true
                ,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            Newtonsoft.Json.JsonSerializer newtonSoftJsonSerializer = JsonHelpers.GetNewtonsoftJsonSerializer();

            var allQuestsKVP = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonDocumentText, options);
            var allQuestsJObject = JObject.Parse(jsonDocumentText);
            _ = allQuestsJObject;

            var questsToAddToProfile = new List<QuestDataClass>();

            foreach (var questKVP in allQuestsKVP)
            {
                var questJson = questKVP.Value.GetRawText();
                var rawQuest = Newtonsoft.Json.JsonConvert.DeserializeObject<RawQuestClass>(questJson, new JsonSerializerSettings() { Converters = JsonHelpers.GetNewtonsoftJsonSerializerConverters() });

                // Check if the quest is already in the profile's quest data
                var questInProfile = _saveProvider.GetPmcProfile(account).QuestsData.Find((x) => x != null && x.Id == questKVP.Key);
                if (questInProfile != null)
                    continue;

                // if it has no conditions just add
                if (rawQuest.Conditions[EFT.Quests.EQuestStatus.AvailableForStart].Count == 0)
                {
                    var questData = new QuestDataClass
                    {
                        Id = questKVP.Key,
                        AvailableAfter = 0,
                        StartTime = 0,
                        CompletedConditions = new HashSet<EFT.MongoID>(),
                        Status = EFT.Quests.EQuestStatus.AvailableForStart,
                        StatusStartTimestamps = new Dictionary<EFT.Quests.EQuestStatus, double>(),
                        Template = allQuestsJObject[questKVP.Key].ToObject<RawQuestClass>(newtonSoftJsonSerializer),
                    };
                    questsToAddToProfile.Add(questData);
                }
            }

            Singleton<IHandbookCategorization>.Create(new HandbookSingletonForQuests());

            var profileQuestData = _saveProvider.GetPmcProfile(account).QuestsData;
            foreach (var questToAdd in questsToAddToProfile)
            {
                foreach (var qtaTemplateConditionsByStatus in questToAdd.Template.Conditions)
                {
                    foreach (var qtaTemplateConditions in qtaTemplateConditionsByStatus.Value)
                    {
                        foreach (var item in qtaTemplateConditions.ChildConditions)
                        {
                        }
                    }
                }
                profileQuestData.Add(questToAdd);
            }



            return profileQuestData.Select(x => x.Template).ToList();
        }

    }

    public class HandbookSingletonForQuests : IHandbookCategorization
    {
        public bool IsCategory(string testId)
        {
            return false;
        }

        public bool IsChildOf(string testId, string parentId)
        {
            return false;
        }
    }
}
