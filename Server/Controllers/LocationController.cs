using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class LocationController : ControllerBase
    {
        private JsonFileSaveProvider _saveProvider;
        private IConfiguration configuration;
        private ILocationService locationService;

        public LocationController(ISaveProvider saveProvider, IConfiguration configuration, ILocationService locationService)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
            this.configuration = configuration;
            this.locationService = locationService;
        }

        /// <summary>
        /// Provides an object of locations (base) and paths between each location
        /// </summary>
        /// <returns></returns>
        [Route("client/locations")]
        [HttpPost]
        public async Task<IActionResult> Locations()
        {
            return new BSGSuccessBodyResult(await Task.FromResult(locationService.LoadLocations()));
        }
    }
}
