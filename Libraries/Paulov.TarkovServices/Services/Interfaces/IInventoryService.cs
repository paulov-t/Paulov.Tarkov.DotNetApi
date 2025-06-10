using Paulov.TarkovModels;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IInventoryService
    {
        public void UpdateInventoryEquipmentId(AccountProfileCharacter profile);
        public void UpdateMongoIds(AccountProfileCharacter profile);

    }
}
