using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.TarkovServices.Services
{
    public class LootGenerationService : ILootGenerationService
    {
        public LootGenerationService(IGlobalsService globalsService, IInventoryService inventoryService, IDatabaseService databaseService)
        {
            GlobalsService = globalsService ?? throw new ArgumentNullException(nameof(globalsService));
            InventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            DatabaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        public IGlobalsService GlobalsService { get; }
        public IInventoryService InventoryService { get; }
        public IDatabaseService DatabaseService { get; }


    }
}
