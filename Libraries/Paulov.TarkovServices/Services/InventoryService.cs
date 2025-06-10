using EFT;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.TarkovServices.Services
{
    public sealed class InventoryService : IInventoryService
    {
        public InventoryService()
        {

        }

        public void UpdateInventoryEquipmentId(AccountProfileCharacter profile)
        {
            var newEquipmentId = MongoID.Generate(false);
            var equipmentId = BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "equipment").ToString();
            var items = BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "items");
            //BSGHelperLibrary.ReflectionHelpers.SetValueOfJsonProperty(profile.Inventory, "equipment", newEquipmentId);

        }

        public void UpdateMongoIds(AccountProfileCharacter profile)
        {
            var newEquipmentId = MongoID.Generate(false).ToString();
            var equipmentId = BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "equipment").ToString();
            var items = BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "items");
        }
    }
}
