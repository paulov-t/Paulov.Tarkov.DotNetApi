using Paulov.TarkovModels;
using FlatItem = GClass1354;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IInventoryService
    {

        public void AddItemToInventory(AccountProfileCharacter profile, FlatItem item);

        public FlatItem AddTemplatedItemToSlot(AccountProfileCharacter profile, string templateId, string slotId, string parentId);

        public IEnumerable<GClass1354> GetChildItemsOfItemId(AccountProfileCharacter profile, string itemId);

        /// <summary>
        /// Get the core EquipmentId
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public string GetEquipmentId(AccountProfileCharacter profile);

        /// <summary>
        /// Gets the items in flat format (including EquipmentId, StashId etc) from the profile's inventory
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public GClass1354[] GetInventoryItems(AccountProfileCharacter profile);


        public void RemoveItemAndChildItemsFromProfile(AccountProfileCharacter profile, string itemId);

        public void RemoveItemFromSlot(AccountProfileCharacter profile, string slotId);

        public void SetInventoryItems(AccountProfileCharacter profile, GClass1354[] items);


        /// <summary>
        /// Update the Inventory Equipment Id to something Unique for each bot
        /// 
        /// </summary>
        /// <param name="profile"></param>
        public void UpdateInventoryEquipmentId(AccountProfileCharacter profile);

        /// <summary>
        /// Update the Ids for all items so that each bot is using "new" items
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="items"></param>
        public void UpdateMongoIds(AccountProfileCharacter profile, GClass1354[] items);

    }
}
