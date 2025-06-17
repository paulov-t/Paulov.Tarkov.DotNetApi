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
            return BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "equipment")?.ToString();
        }

        public string GetStashId(AccountProfileCharacter profile)
        {
            return BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "stash")?.ToString();
        }

        public string GetQuestRaidItemsId(AccountProfileCharacter profile)
        {
            return BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "questRaidItems")?.ToString();
        }

        public string GetQuestStashItemsId(AccountProfileCharacter profile)
        {
            return BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "questStashItems")?.ToString();
        }

        public string GetSortingTableId(AccountProfileCharacter profile)
        {
            return BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "sortingTable")?.ToString();
        }

        public Dictionary<EAreaType, MongoID> GetHideoutAreaStashes(AccountProfileCharacter profile)
        {
            return BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "hideoutAreaStashes") as Dictionary<EAreaType, MongoID>;
        }

        public Dictionary<EFT.InventoryLogic.EBoundItem, MongoID> GetFastPanel(AccountProfileCharacter profile)
        {
            return BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "fastPanel") as Dictionary<EFT.InventoryLogic.EBoundItem, MongoID>;
        }

        public List<MongoID> GetFavoriteItems(AccountProfileCharacter profile)
        {
            return BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "favoriteItems") as List<MongoID>;
        }

        public string GetHideoutCustomizationStashId(AccountProfileCharacter profile)
        {
            return BSGHelperLibrary.ReflectionHelpers.GetValueOfJsonProperty(profile.Inventory, "hideoutCustomizationStashId")?.ToString();
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

            bool generateEquipmentInItems = equipmentId == null;

            var items = GetInventoryItems(profile);
            foreach (var item in items)
            {
                if (item._id == equipmentId)
                    item._id = newEquipmentId;

                if (item.parentId == equipmentId)
                    item.parentId = newEquipmentId;
            }
            BSGHelperLibrary.ReflectionHelpers.SetValueOfJsonProperty(profile.Inventory, "equipment", newEquipmentId);
            var lstItems = items.ToList();

            if (generateEquipmentInItems)
                lstItems.Add(new FlatItem() { _id = newEquipmentId, _tpl = "55d7217a4bdc2d86028b456d" });

            SetInventoryItems(profile, lstItems.ToArray());

        }

        public List<FlatItem> UpdateMongoIds(AccountProfileCharacter character, List<FlatItem> items)
        {
            List<(string from, string to)> renamedId = new List<(string, string)>();

            var equipmentId = GetEquipmentId(character);
            var questRaidItemsId = GetQuestRaidItemsId(character);
            var questStashItemsId = GetQuestStashItemsId(character);
            var sortingTableId = GetSortingTableId(character);
            var stashId = GetStashId(character);
            var hideoutCustomizationStashId = GetHideoutCustomizationStashId(character);
            // Pass 1: Generate new IDs for each item
            foreach (var item in items)
            {
                // Must not remap the equipment, questRaidItems, questStashItems, sortingTable, stash and hideoutCustomizationStashId
                if (item._id.ToString() == equipmentId ||
                    item._id.ToString() == questRaidItemsId ||
                    item._id.ToString() == questStashItemsId ||
                    item._id.ToString() == sortingTableId ||
                    item._id.ToString() == stashId ||
                    item._id.ToString() == hideoutCustomizationStashId)
                {
                    continue;
                }

                var previousId = item._id;
                item._id = MongoID.Generate(false).ToString();
                renamedId.Add((from: previousId, to: item._id));
            }

            // Pass 2: Update parentId to the new IDs
            foreach (var item in items.Where(x => x.parentId.HasValue))
            {
                // Find the previous ID in the renamedId list
                if (!renamedId.Any(x => x.from == item.parentId))
                    continue;

                item.parentId = renamedId.First(x => x.from == item.parentId).to;
            }

            // Pass 3: Update parentId to the new IDs
            foreach (var item in items.Where(x => x.parentId.HasValue))
            {
                // Find the previous ID in the renamedId list
                if (!renamedId.Any(x => x.from == item.parentId))
                    continue;

                item.parentId = renamedId.First(x => x.from == item.parentId).to;
            }

            return items;
        }
    }
}
