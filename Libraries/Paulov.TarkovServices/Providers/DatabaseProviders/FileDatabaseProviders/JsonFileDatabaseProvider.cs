using Newtonsoft.Json;
using Paulov.TarkovServices.Models;
using Paulov.TarkovServices.Providers.Interfaces;
using System.Data;

namespace Paulov.TarkovServices.Providers.DatabaseProviders.FileDatabaseProviders
{
    /// <summary>
    /// Provides functionality to interact with a JSON file-based database. 
    /// </summary>
    /// <remarks>This can only serve an instance or list of one type with properties per Provider. This class implements the <see cref="IDatabaseProvider"/> interface to manage database
    /// operations using a JSON file as the underlying storage. It allows connecting to a JSON file, executing commands,
    /// retrieving data, and accessing entry streams.</remarks>
    public class JsonFileDatabaseProvider : IDatabaseProvider
    {
        private string FileName { get; set; }

        public List<EntryModel> Entries => throw new NotImplementedException();

        /// <summary>
        /// Setup the FileName of this File Database Provider to the "connectionString"
        /// </summary>
        /// <param name="connectionString"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Connect(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            FileName = connectionString;
            if (!FileName.EndsWith(".json"))
                FileName += ".json";
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public void Disconnect()
        {
        }

        /// <summary>
        /// Writes the "query" to the file
        /// </summary>
        /// <param name="query"></param>
        public void ExecuteCommand(string query)
        {
            File.WriteAllText(FileName, query);
        }

        public DataTable GetData(string query)
        {
            var json = File.ReadAllText(FileName);

            DataTable dataTable = new();
            if (string.IsNullOrWhiteSpace(json))
            {
                return dataTable;
            }
            dataTable = JsonConvert.DeserializeObject<DataTable>(json);
            return dataTable;
        }

        public Stream GetEntryStream(string entryName)
        {
            return new MemoryStream(File.ReadAllBytes(FileName));
        }
    }
}
