using Paulov.TarkovServices.Models;
using Paulov.TarkovServices.Providers.Interfaces;
using System.Data;

namespace Paulov.TarkovServices.Providers.DatabaseProviders.FileDatabaseProviders
{
    public sealed class JsonFileCollectionDatabaseProvider : IDatabaseProvider
    {
        public List<EntryModel> Entries { get; } = new List<EntryModel>();

        public void Connect(string connectionString)
        {
            // Load all JSON files from the specified directory
            var directory = new DirectoryInfo(connectionString);
            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"The directory '{connectionString}' does not exist.");
            }
            var jsonFiles = directory.GetFiles("*.json", new EnumerationOptions() { RecurseSubdirectories = true });
            foreach (var file in jsonFiles)
            {
                Entries.Add(new EntryModel(file.Name, file.FullName.Replace(connectionString, "").Replace("\\", "/"), this));
            }
        }

        public void Disconnect()
        {
        }

        public void ExecuteCommand(string query)
        {
        }

        public DataTable GetData(string query)
        {
            return null;
        }

        public Stream GetEntryStream(string entryName)
        {
            // Find the entry by name
            var entry = Entries.FirstOrDefault(e =>
                e.Name.Equals(entryName, StringComparison.OrdinalIgnoreCase)
                || e.FullName.Equals(entryName, StringComparison.OrdinalIgnoreCase)
                || e.FullName.Equals(entryName.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase)
                );
            if (entry == null)
            {
                throw new FileNotFoundException($"The entry '{entryName}' was not found in the database.");
            }


            return new MemoryStream(File.ReadAllBytes(entry.FullName));
        }
    }
}
