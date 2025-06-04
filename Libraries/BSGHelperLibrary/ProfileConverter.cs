using EFT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SIT.BSGHelperLibrary
{
    public class ProfileConverter : JsonConverter<EFT.Profile>
    {
        public override bool CanRead => base.CanRead;
        public override bool CanWrite => base.CanWrite;

        public override void WriteJson(JsonWriter writer, EFT.Profile value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override EFT.Profile ReadJson(JsonReader reader, Type objectType, EFT.Profile existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject profile = null;

            try
            {
                profile = JObject.Load(reader);
            }
            catch (Exception)
            {
                return null;
            }
            existingValue = new EFT.Profile(new GClass2030()
            {
                Id = profile["_id"].ToString(),
                AccountId = profile["aid"].ToString(),
                AchievementsData = new System.Collections.Generic.Dictionary<EFT.MongoID, int>(),
                Customization = BSGJsonHelpers.SITParseJson<Dictionary<EBodyModelPart, MongoID>>(profile["Customization"].ToString()),
                Encyclopedia = BSGJsonHelpers.SITParseJson<Dictionary<MongoID, bool>>(profile["Encyclopedia"].ToString()),
                Hideout = BSGJsonHelpers.SITParseJson<GClass2002>(profile["Hideout"].ToString()),
                Inventory = BSGJsonHelpers.SITParseJson<GClass1717>(profile["Inventory"].ToString()),
                TradersInfo = BSGJsonHelpers.SITParseJson<Dictionary<MongoID, GClass2029>>(profile["TradersInfo"].ToString()),
                WishList = BSGJsonHelpers.SITParseJson<Dictionary<MongoID, byte>>(profile["WishList"].ToString())
            });

            return existingValue;
        }
    }

    //public class InventoryConverter : JsonConverter<EFT.InventoryLogic.Inventory>
    //{
    //    public override Inventory ReadJson(JsonReader reader, Type objectType, Inventory existingValue, bool hasExistingValue, JsonSerializer serializer)
    //    {
    //        JObject Inventory = JObject.Load(reader);
    //        if (existingValue == null)
    //            existingValue = new EFT.InventoryLogic.Inventory();

    //        // Equipment Template is a GClass. Lets get that dynamically.
    //        var equipmentConstructor = typeof(EquipmentClass).GetConstructors().First();
    //        var paramType = equipmentConstructor.GetParameters()[1].ParameterType;
    //        var equipmentTemplate = Activator.CreateInstance(paramType);
    //        existingValue.Equipment = (EquipmentClass)equipmentConstructor.Invoke(new object[] { Inventory["equipment"].ToString(), equipmentTemplate });
    //        return existingValue;
    //    }

    //    public override void WriteJson(JsonWriter writer, Inventory value, JsonSerializer serializer)
    //    {
    //    }

    //}
}