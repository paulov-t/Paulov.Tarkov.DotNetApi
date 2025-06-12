using EFT;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Services.Interfaces;
using FlatItem = GClass1354;

namespace Paulov.TarkovServices.Services
{
    public sealed class InventoryService : IInventoryService
    {
        public InventoryService()
        {

        }

        public void AddItemToInventory(AccountProfileCharacter profile, FlatItem item)
        {
            var items = GetInventoryItems(profile).ToList();

            // Do a check to see if the code is attempting to add another item to the same equipment slot
            var slotId = item.slotId;
            if (!string.IsNullOrEmpty(slotId) && Enum.TryParse<EFT.InventoryLogic.EquipmentSlot>(slotId, out _) && items.Any(x => x.slotId == slotId))
            {
                throw new Exception($"Item already exists in Inventory in {slotId}");
            }

            items.Add(item);
            SetInventoryItems(profile, items.ToArray());
        }

        public FlatItem AddTemplatedItemToSlot(AccountProfileCharacter profile, string templateId, string slotId, string parentId)
        {
            if (string.IsNullOrEmpty(templateId))
                throw new ArgumentNullException(nameof(templateId));

            if (string.IsNullOrEmpty(slotId))
                throw new ArgumentNullException(nameof(slotId));

            var resultingNewItem = new FlatItem()
            {
                _id = MongoID.Generate(false).ToString(),
                _tpl = templateId,
                slotId = slotId,
                parentId = !string.IsNullOrEmpty(parentId) ? parentId : GetEquipmentId(profile)
            };

            return resultingNewItem;
        }

        public IEnumerable<FlatItem> GetChildItemsOfItemId(AccountProfileCharacter profile, string itemId)
        {
            foreach (var item in GetInventoryItems(profile))
            {
                if (item.parentId == itemId)
                {
                    yield return item;

                    // Child items of the child item recurse
                    foreach (var childItem in GetChildItemsOfItemId(profile, item._id))
                    {
                        yield return childItem;
                    }
                }

            }
        }

        public string GetEquipmentId(AccountProfileCharacter profile)
        {
            return BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "equipment").ToString();
        }

        public FlatItem[] GetInventoryItems(AccountProfileCharacter profile)
        {
            var items = BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "items") as FlatItem[];
            return items;
        }

        public void RemoveItemAndChildItemsFromProfile(AccountProfileCharacter profile, string itemId)
        {
            if (profile == null)
                return;

            if (itemId == null)
                return;


            var items = GetInventoryItems(profile).ToList();
            var itemsToRemove = new List<string>() { itemId };
            var equipmentId = GetEquipmentId(profile);

            var childItems = GetChildItemsOfItemId(profile, itemId);
            foreach (var item in childItems)
            {
                if (item._id == equipmentId)
                    continue;

                itemsToRemove.Add(item._id);
            }

            items = items.Where(x => !itemsToRemove.Contains(x._id)).ToList();

            SetInventoryItems(profile, items.ToArray());
        }

        public void RemoveItemFromSlot(AccountProfileCharacter profile, string slotId)
        {
            if (profile == null)
                return;

            if (slotId == null)
                return;

            var inventoryItems = GetInventoryItems(profile);

            var item = inventoryItems.FirstOrDefault(x => x.slotId == slotId);
            if (item == null)
                return;

            RemoveItemAndChildItemsFromProfile(profile, item._id);
        }

        public void SetInventoryItems(AccountProfileCharacter profile, FlatItem[] items)
        {
            BSGHelperLibrary.ReflectionHelpers.SetValueOfJsonProperty(profile.Inventory, "items", items);
        }

        public void UpdateInventoryEquipmentId(AccountProfileCharacter profile)
        {
            var newEquipmentId = MongoID.Generate(false);
            var equipmentId = GetEquipmentId(profile);
            var items = GetInventoryItems(profile);
            foreach (var item in items)
            {
                if (item._id == equipmentId)
                    item._id = newEquipmentId;

                if (item.parentId == equipmentId)
                    item.parentId = newEquipmentId;
            }
            BSGHelperLibrary.ReflectionHelpers.SetValueOfJsonProperty(profile.Inventory, "equipment", newEquipmentId);
            SetInventoryItems(profile, items);

        }

        public void UpdateMongoIds(AccountProfileCharacter profile, FlatItem[] items)
        {

        }
    }
}
