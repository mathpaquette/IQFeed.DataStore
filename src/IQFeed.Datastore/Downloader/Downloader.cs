using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IQFeed.CSharpApiClient.Lookup;
using IQFeed.CSharpApiClient.Lookup.Historical.Enums;
using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;
using IQFeed.Datastore.Writer;

namespace IQFeed.Datastore.Downloader
{
    public class Downloader
    {
        private readonly FileWriter _fileWriter;
        private readonly LookupClient<double> _lookupClient;
        private readonly Task[] _tasks;

        public Downloader(
            FileWriter fileWriter,
            LookupClient<double> lookupClient,
            int numberOfClients)
        {
            _fileWriter = fileWriter;
            _lookupClient = lookupClient;
            _tasks = new Task[numberOfClients];
        }

        public void GetMarketData(IEnumerable<MarketSymbol> symbols, IEnumerable<Resolution> resolutions, DateTime startDate, DateTime endDate, bool fundamental)
        {
            var requests = new ConcurrentQueue<MarketRequest>(resolutions.SelectMany(resolution => symbols.Select(symbol => new MarketRequest(symbol, resolution))));

            for (int i = 0; i < _tasks.Length; i++)
            {
                _tasks[i] = Task.Run(async () =>
                {
                    while (requests.TryDequeue(out var request))
                    {
                        Console.WriteLine("Downloading");
                        var marketFile = await GetMarketFileAsync(request, startDate, endDate);
                        _fileWriter.Write(marketFile);
                    }
                });
            }

            Task.WaitAll(_tasks);
        }

        private async Task<MarketFile> GetMarketFileAsync(MarketRequest request, DateTime startDate, DateTime endDate)
        {
            string filename;
            switch (request.Resolution)
            {
                case Resolution.Tick:
                    filename = await _lookupClient.Historical.File.GetHistoryTickTimeframeAsync(request.MarketSymbol.Symbol, startDate, endDate, dataDirection: DataDirection.Oldest);
                    break;
                default:
                    filename = "";
                    break;
            }

            return new MarketFile(request.MarketSymbol, filename, request.Resolution);
        }

        public IEnumerable<MarketSymbol> GetAllMarketSymbols()
        {
            return _lookupClient.Symbol.GetAllMarketSymbols();
        }
    }
}