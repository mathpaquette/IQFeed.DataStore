using System;
using System.Linq;
using IQFeed.CSharpApiClient.Lookup;
using IQFeed.Datastore.Downloader;

namespace IQFeed.Datastore.Reader
{
    class Program
    {
        static void Main(string[] args)
        {
            // example
            var lookupClient = LookupClientFactory.CreateNew();
            var symbol = lookupClient.Symbol.GetAllMarketSymbols().First(x => x.Symbol == "AAPL");
            
            var fileReader = new FileReader(@"C:\data", symbol, new DateTime(2020, 03, 30), DataType.Tick);
            var rows = fileReader.Parse().ToList();
        }
    }
}
