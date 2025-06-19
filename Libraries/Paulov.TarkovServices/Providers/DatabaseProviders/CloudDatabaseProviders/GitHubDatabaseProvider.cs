using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices.Models;
using Paulov.TarkovServices.Providers.Interfaces;
using System.Data;
using System.Text;

namespace Paulov.TarkovServices.Providers.DatabaseProviders.CloudDatabaseProviders
{
    public sealed class GitHubDatabaseProvider : IDatabaseProvider
    {
        public List<EntryModel> Entries { get; set; } = new List<EntryModel>();

        HttpClient client;


        public GitHubDatabaseProvider(IConfiguration configuration)
        {
            Connect(configuration["GitHubDatabaseUrl"]);
        }

        public void Connect(string connectionString)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri($"https://raw.githubusercontent.com/{connectionString}/refs/heads/master/");
            client.GetAsync("database/globals.json").ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully && task.Result.IsSuccessStatusCode)
                {
                    var response = task.Result.Content.ReadAsStringAsync().Result;
                    var globals = JObject.Parse(response);
                    Entries.Add(new EntryModel("globals.json", "database/globals.json", this));
                }
            });
        }

        public void Disconnect()
        {
        }

        public void ExecuteCommand(string query)
        {
        }

        public DataTable GetData(string query)
        {
            return new DataTable();
        }

        public Stream GetEntryStream(string entryName)
        {
            var response = client.GetAsync($"{entryName}").GetAwaiter().GetResult().Content.ReadAsStringAsync().Result;

            return new MemoryStream(Encoding.UTF8.GetBytes(response));
        }
    }
}
