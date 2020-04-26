using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Serilog;
using Serilog.Events;

namespace IQFeed.DataStore.Downloader
{
    class Program
    {
        public class Options
        {
            [Option('t', "tickers", Separator = ',', HelpText = "List of tickers comma-separated.", Group = "TICKER")]
            public IEnumerable<string> Tickers { get; set; }

            [Option('g', "group", HelpText = "Ticker group.", Group = "TICKER")]
            public string Group { get; set; }

            [Option('s', "from-date", SetName = "DateRange", Required = true, HelpText = @"Format: ""YYYY-MM-DD HH:MM:SS""")]
            public DateTime FromDate { get; set; }

            [Option('e', "to-date", SetName = "DateRange", Required = true, HelpText = @"Format: ""YYYY-MM-DD HH:MM:SS""")]
            public DateTime ToDate { get; set; }

            [Option('c', "today", SetName = "DateNow", Required = true, HelpText = "Download data for current day.")]
            public bool Today { get; set; }

            [Option('d', "data", Default = new[] { DataType.Tick, DataType.Second, DataType.Minute, DataType.Hour, DataType.Daily, DataType.Weekly, DataType.Fundamental }, Separator = ',', HelpText = "Data types comma-separated.")]
            public IEnumerable<DataType> DataTypes { get; set; }

            [Option('p', "path", HelpText = "Download path.", Default = @".\")]
            public string Path { get; set; }

            [Option('n', "clients", Default = 8, HelpText = "Number of concurrent clients.")]
            public int Clients { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Start)
                .WithNotParsed(Error);
        }

        private static void Start(Options options)
        {
            CreateLogger(options.Path);

            // filter group
            var marketSymbolFilterService = new MarketSymbolFilterService();
            FilterDesc filterDesc = null;
            if (!string.IsNullOrEmpty(options.Group))
            {
                filterDesc = marketSymbolFilterService.GetFilterDescByGroupId(options.Group);
                if (filterDesc == null)
                {
                    throw new Exception("Unable to find group from");
                }
            }

            var numberOfClients = options.Clients;
            var path = options.Path;
            var dataTypes = options.DataTypes;
            var startDate = options.Today ? DateTime.Now.Date : options.FromDate;
            var endDate = options.Today ? DateTime.Now.Date.AddDays(1).AddSeconds(-1) : options.ToDate;
            var downloader = DownloaderFactory.CreateNew(path, numberOfClients);

            var symbols = filterDesc != null
                ? filterDesc.Func(downloader.GetAllMarketSymbols()).ToList()
                : downloader.GetAllMarketSymbols().Where(x => options.Tickers.Contains(x.Symbol)).ToList();

            downloader.GetMarketData(symbols, dataTypes, startDate, endDate);
        }

        private static void Error(IEnumerable<Error> errors)
        {
            var commandLineExample = @"--tickers=AAPL,SPY --data=Tick,Fundamental --from-date=""2020-01-30 04:00:00"" --to-date=""2020-01-30 20:00:00"" --clients=4";
            Console.WriteLine($"Usage example: {commandLineExample}");
        }

        private static void CreateLogger(string path)
        {
            var logFile = Path.Combine(path, "logs/downloader/log.txt");
            var errorFile = Path.Combine(path, "logs/downloader/error.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.Logger(l => l.Filter.ByExcluding(e => e.Level == LogEventLevel.Error)
                    .WriteTo.File(logFile, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                    .WriteTo.File(errorFile, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true))
                .CreateLogger();
        }
    }
}