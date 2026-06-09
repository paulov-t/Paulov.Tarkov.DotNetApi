using Microsoft.Extensions.Configuration;
using Paulov.TarkovServices.Models;
using Paulov.TarkovServices.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulov.TarkovServices.Providers.DatabaseProviders.ApiProviders
{
    /// <summary>
    /// This has not been implemented yet.
    /// </summary>
    public class TarkovDevApiProvider : IDatabaseProvider
    {
        public List<EntryModel> Entries => new List<EntryModel>();

        HttpClient _client;

        public TarkovDevApiProvider(IConfiguration configuration)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri($"https://api.tarkov.dev/graphql");
            _client.Timeout = new TimeSpan(0, 0, 10);
        }

        public void Connect(string connectionString)
        {
        }

        public void Disconnect()
        {
        }

        public void ExecuteCommand(string query)
        {
            return;
        }

        public DataTable GetData(string query)
        {
            return null;
        }

        public Stream GetEntryStream(string entryName)
        {
            return null;
        }
    }
}
