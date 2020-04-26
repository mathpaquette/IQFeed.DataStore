using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IQFeed.CSharpApiClient.Lookup;
using IQFeed.CSharpApiClient.Lookup.Common;
using IQFeed.CSharpApiClient.Lookup.Historical.Enums;
using IQFeed.CSharpApiClient.Lookup.Historical.Messages;
using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;
using IQFeed.CSharpApiClient.Streaming.Level1;
using Serilog;

namespace IQFeed.DataStore.Downloader
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
            var marketSymbolPath = Path.Combine(_dataDirectory, "symbol", $"{DateTime.Now.ToString(DataStore.DateFormat)}_mktsymbols_v2.zip");
            var marketSymbolDirectory = Path.GetDirectoryName(marketSymbolPath);
            
            if (marketSymbolDirectory != null && !Directory.Exists(marketSymbolDirectory))
            {
                Log.Information($"Downloader.GetAllMarketSymbols(): Create directory {marketSymbolDirectory}");
                Directory.CreateDirectory(marketSymbolDirectory);
            }

            if (!File.Exists(marketSymbolPath))
            {
                Log.Information($"Downloader.GetAllMarketSymbols(): Downloading Market Symbols to {marketSymbolPath}");
                using (var client = new WebClient())
                {
                    client.DownloadFile("http://www.dtniq.com/product/mktsymbols_v2.zip", marketSymbolPath);
                }
            }

            return MarketSymbolReader.GetMarketSymbols(marketSymbolPath);
        }

        private async Task ProcessRequest(ConcurrentQueue<MarketRequest> requests)
        {
            while (requests.TryDequeue(out var request))
            {
                try
                {
                    FileWriter fileWriter;
                    Log.Information($"Downloader.ProcessRequest(): Downloading {request.DataType} for {request.MarketSymbol.Symbol} from {request.StartDate} to {request.EndDate}");

                    switch (request.DataType)
                    {
                        case DataType.Fundamental:
                            var fundamentalMessage = await _level1Client.GetFundamentalSnapshotAsync(request.MarketSymbol.Symbol);
                            var fundamentalData = new FundamentalData(DateTime.Now.Date, fundamentalMessage.ToCsv());
                            fileWriter = new FileWriter(_dataDirectory, request.MarketSymbol, DataType.Fundamental);
                            fileWriter.Write(new List<HistoricalData>() { fundamentalData });
                            break;
                        case DataType.Tick:
                        case DataType.Daily:
                            var filename = await GetMarketFilename(request);
                            fileWriter = new FileWriter(_dataDirectory, request.MarketSymbol, request.DataType);
                            var historicalData = GetData(request, filename);
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
            switch (request.DataType)
            {
                case DataType.Tick:
                    return _lookupClient.Historical.File.GetHistoryTickTimeframeAsync(request.MarketSymbol.Symbol, request.StartDate, request.EndDate, dataDirection: DataDirection.Oldest);
                case DataType.Daily:
                    return _lookupClient.Historical.File.GetHistoryDailyTimeframeAsync(request.MarketSymbol.Symbol, request.StartDate, request.EndDate, dataDirection: DataDirection.Oldest);
                default:
                    throw new NotSupportedException();
            }
        }

        private IEnumerable<HistoricalData> GetData(MarketRequest request, string filename)
        {
            switch (request.DataType)
            {
                case DataType.Tick:
                    return LookupMessageFileParser.ParseFromFile(line => new HistoricalData(TickMessage.Parse(line).Timestamp, line), filename);
                case DataType.Daily:
                    return LookupMessageFileParser.ParseFromFile(line => new HistoricalData(DailyWeeklyMonthlyMessage.Parse(line).Timestamp, line), filename);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}