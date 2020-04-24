using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IQFeed.CSharpApiClient.Lookup;
using IQFeed.CSharpApiClient.Lookup.Common;
using IQFeed.CSharpApiClient.Lookup.Historical.Enums;
using IQFeed.CSharpApiClient.Lookup.Historical.Messages;
using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;
using IQFeed.CSharpApiClient.Streaming.Level1;
using Serilog;

namespace IQFeed.Datastore.Downloader
{
    public class Downloader
    {
        private readonly LookupClient<double> _lookupClient;
        private readonly Level1Client<double> _level1Client;
        private readonly Task[] _tasks;
        private readonly string _dataDirectory;

        public Downloader(string dataDirectory, LookupClient<double> lookupClient, Level1Client<double> level1Client, int numberOfClients)
        {
            _level1Client = level1Client;
            _dataDirectory = dataDirectory;
            _lookupClient = lookupClient;
            _tasks = new Task[numberOfClients];
        }

        public void GetMarketData(IEnumerable<MarketSymbol> symbols, IEnumerable<DataType> dataTypes, DateTime startDate, DateTime endDate)
        {
            var marketRequests = dataTypes
                .SelectMany(resolution =>
                    symbols.Select(symbol => new MarketRequest(symbol, resolution, startDate, endDate))).ToList();

            var requests = new ConcurrentQueue<MarketRequest>(marketRequests);

            for (int i = 0; i < _tasks.Length; i++)
            {
                _tasks[i] = Task.Run(async () => await ProcessRequest(requests));
            }

            Task.WaitAll(_tasks);
        }

        public IEnumerable<MarketSymbol> GetAllMarketSymbols()
        {
            return _lookupClient.Symbol.GetAllMarketSymbols();
        }

        private async Task ProcessRequest(ConcurrentQueue<MarketRequest> requests)
        {
            while (requests.TryDequeue(out var request))
            {
                try
                {
                    FileWriter fileWriter;

                    switch (request.DataType)
                    {
                        case DataType.Fundamental:
                            Log.Information($"Downloading {request.DataType} for {request.MarketSymbol.Symbol}");
                            var fundamentalMessage = await _level1Client.GetFundamentalSnapshotAsync(request.MarketSymbol.Symbol);
                            var fundamentalData = new FundamentalData(DateTime.Now.Date, fundamentalMessage.ToCsv());
                            fileWriter = new FileWriter(_dataDirectory, request.MarketSymbol, DataType.Fundamental);
                            fileWriter.Write(new List<HistoricalData>() { fundamentalData });
                            break;

                        case DataType.Tick:
                            var filename = await GetMarketFilename(request);
                            fileWriter = new FileWriter(_dataDirectory, request.MarketSymbol, request.DataType);
                            var historicalData = LookupMessageFileParser.ParseFromFile(line => new HistoricalData(TickMessage.Parse(line).Timestamp, line), filename);
                            fileWriter.Write(historicalData);
                            File.Delete(filename);
                            break;
                        default:
                            throw new NotSupportedException("DataType not yet supported.");
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"{request.MarketSymbol.Symbol} Error: {e}");
                }
            }
        }

        private Task<string> GetMarketFilename(MarketRequest request)
        {
            Log.Information($"Downloading {request.DataType} for {request.MarketSymbol.Symbol} from {request.StartDate} to {request.EndDate}");

            switch (request.DataType)
            {
                case DataType.Tick:
                    return _lookupClient.Historical.File.GetHistoryTickTimeframeAsync(request.MarketSymbol.Symbol, request.StartDate, request.EndDate, dataDirection: DataDirection.Oldest);
                default:
                    return null;
                    break;
            }
        }
    }
}