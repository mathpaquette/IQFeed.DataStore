using System;
using System.Collections.Generic;
using System.Linq;
using IQFeed.CSharpApiClient.Lookup.Symbol;
using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;
using IQFeed.Datastore.Downloader;
using IQFeed.Datastore.Writer;

namespace IQFeed.Datastore.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var downloader = DownloaderFactory.CreateNew(@"C:\data", 8);

            var exchanges = new List<string>() { "NYSE", "NASDAQ", "NYSE_AMERICAN", "BATS", "IEX" };

            //var symbols = downloader.GetAllMarketSymbols()
            //    .Where(x => x.SecurityType == "EQUITY" && exchanges.Contains(x.Exchange) && x.ListedMarket != "OTC").ToList();

            var symbols = downloader.GetAllMarketSymbols().Where(x => x.Symbol == "NEPT");

            var resolutions = new List<Resolution>() {Resolution.Tick};
            var startDate = new DateTime(2019, 01, 01);
            var endDate = DateTime.Now.Date;

            downloader.GetMarketData(symbols, resolutions, startDate, endDate, true);
            Console.ReadLine();
        }
    }
}
