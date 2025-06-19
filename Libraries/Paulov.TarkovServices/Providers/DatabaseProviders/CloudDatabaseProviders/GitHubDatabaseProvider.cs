using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices.Models;
using Paulov.TarkovServices.Providers.Interfaces;
using System.Data;
using System.Text;

namespace Paulov.TarkovServices.Providers.DatabaseProviders.CloudDatabaseProviders
{
    /// <summary>
    /// Provides access to a GitHub-hosted database by interacting with raw content files.
    /// </summary>
    /// <remarks>This class connects to a GitHub repository and retrieves database entries stored as raw
    /// files. It implements the <see cref="IDatabaseProvider"/> interface, allowing integration with systems that
    /// require database-like functionality.
    /// 
    /// This has the requirement for configuration to include a GitHub repository URL in the format "username/repository" stored in appsettings.json as GitHubDatabaseUrl.
    /// This has the requirement for configuration to include a GitHub Authorization stored in appsettings.json as GitHubAuthToken.
    /// 
    /// 
    /// 
    /// </remarks>
    /// 
    public sealed class GitHubDatabaseProvider : IDatabaseProvider
    {
        public List<EntryModel> Entries { get; } = new List<EntryModel>();

        HttpClient _client;

        IConfiguration _configuration;

        string _connectionString;

        public GitHubDatabaseProvider(IConfiguration configuration)
        {
            this._configuration = configuration;
            Connect(configuration["GitHubDatabaseUrl"]);
        }

        public void Connect(string connectionString)
        {
            _connectionString = connectionString;
            _client = new HttpClient();
            _client.BaseAddress = new Uri($"https://raw.githubusercontent.com/{connectionString}/refs/heads/master/");
            _client.GetAsync("database/globals.json").ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully && task.Result.IsSuccessStatusCode)
                {
                    var response = task.Result.Content.ReadAsStringAsync().Result;
                    var globals = JObject.Parse(response);
                    Entries.Add(new EntryModel("globals.json", "database/globals.json", this));
                }
            });


            var apiEntries = new HttpClient();
            apiEntries.BaseAddress = new Uri($"https://api.github.com/");
            apiEntries.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["GitHubAuthToken"]);
            apiEntries.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            apiEntries.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Paulov.TarkovServices", "1.0"));
            var r = apiEntries.GetAsync($"/repos/{connectionString}/contents").Result.Content.ReadAsStringAsync().Result;
            RecursiveAddEntries(apiEntries, JArray.Parse(r));
            _ = Entries;
        }

        /// <summary>
        /// Recursively adds entries to the Entries list from the provided JArray of entries. This is based on the GitHub API response structure.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="entries"></param>
        private void RecursiveAddEntries(HttpClient client, JArray entries)
        {
            foreach (var entry in entries)
            {
                if (entry["type"].ToString() == "file")
                {
                    var name = entry["name"].ToString();
                    var fullName = entry["path"].ToString();
                    Entries.Add(new EntryModel(name, fullName, this));
                }
                else if (entry["type"].ToString() == "dir")
                {
                    var r = client.GetAsync($"/repos/{_connectionString}/contents/{entry["path"]}").Result.Content.ReadAsStringAsync().Result;
                    if (r.StartsWith("["))
                        RecursiveAddEntries(client, JArray.Parse(r));
                }
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
            return new DataTable();
        }

        public Stream GetEntryStream(string entryName)
        {
            var response = _client.GetAsync($"{entryName}").GetAwaiter().GetResult().Content.ReadAsStringAsync().Result;

            return new MemoryStream(Encoding.UTF8.GetBytes(response));
        }
    }
}
