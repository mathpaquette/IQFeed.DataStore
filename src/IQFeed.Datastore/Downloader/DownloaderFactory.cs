using System.Configuration;
using IQFeed.CSharpApiClient;
using IQFeed.CSharpApiClient.Lookup;
using IQFeed.Datastore.Writer;

namespace IQFeed.Datastore.Downloader
{
    public class DownloaderFactory
    {
        public static Downloader CreateNew(string dataDirectory, int numberOfClients)
        {
            IQFeedLauncher.Start();
            var lookupClient = LookupClientFactory.CreateNew(numberOfClients);
            lookupClient.Connect();
            
            var fileWriter = new FileWriter(dataDirectory);

            return new Downloader(fileWriter, lookupClient, numberOfClients);
        }
    }
}