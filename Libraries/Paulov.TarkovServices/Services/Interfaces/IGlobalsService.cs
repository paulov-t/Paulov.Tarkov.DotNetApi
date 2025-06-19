using Newtonsoft.Json.Linq;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IGlobalsService
    {
        /// <summary>
        /// Loads global configuration settings into the singleton instance.
        /// </summary>
        /// <remarks>This method initializes the singleton with global settings, ensuring that the instance
        /// is populated with the required configuration data. Call this method before accessing the singleton to ensure
        /// it is properly initialized.</remarks>
        JObject LoadGlobalsIntoComfortSingleton();
        /// <summary>
        /// Loads globals from a file.
        /// </summary>
        /// <returns>A JSON object containing the loaded globals.</returns>
        JObject LoadGlobals();
    }
}
