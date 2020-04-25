using System;
using IQFeed.CSharpApiClient;
using IQFeed.CSharpApiClient.Lookup;
using IQFeed.CSharpApiClient.Streaming.Level1;

namespace IQFeed.DataStore.Downloader
{
    public class DownloaderFactory
    {
        public static Downloader CreateNew(string dataDirectory, int numberOfClients)
        {
            IQFeedLauncher.Start();
            var lookupClient = LookupClientFactory.CreateNew("localhost", IQFeedDefault.LookupPort, numberOfClients, TimeSpan.FromMinutes(20));
            lookupClient.Connect();

            var level1Client = Level1ClientFactory.CreateNew();
            level1Client.Connect();

            return new Downloader(dataDirectory, lookupClient, level1Client, numberOfClients);
        }
    }
}