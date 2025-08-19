using Paulov.TarkovServices.Models;
using Paulov.TarkovServices.Providers.Interfaces;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace Paulov.TarkovServices.Providers.DatabaseProviders.FileDatabaseProviders
{
    public sealed class JsonFileCollectionDatabaseProvider : IDatabaseProvider
    {
        public List<EntryModel> Entries { get; } = new List<EntryModel>();

        string parentPath;

        public JsonFileCollectionDatabaseProvider()
        {
            parentPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            Connect(parentPath);
        }

        public void Connect(string connectionString)
        {
            // Load all JSON files from the specified directory
            var directory = new DirectoryInfo(connectionString);
            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"The directory '{connectionString}' does not exist.");
            }

            if (parentPath != connectionString)
                parentPath = connectionString;

            var jsonFiles = directory.GetFiles("*.json", new EnumerationOptions() { RecurseSubdirectories = true });
            foreach (var file in jsonFiles)
            {
                var fullName = file.FullName.Replace(connectionString, "").Replace("\\", "/");
                if (fullName.StartsWith('/'))
                    fullName = fullName.Substring(1, fullName.Length - 1);

                Entries.Add(new EntryModel(file.Name, fullName, this));
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
#if DEBUG
                Debug.WriteLine($"The entry '{entryName}' was not found in the database.");
#endif
                //throw new FileNotFoundException($"The entry '{entryName}' was not found in the database.");
                return null;
            }


            return new MemoryStream(File.ReadAllBytes(Path.Combine(parentPath, entry.FullName)));
        }
    }
}
