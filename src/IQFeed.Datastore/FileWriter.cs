using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;
using IQFeed.DataStore.Downloader;
using Serilog;

namespace IQFeed.DataStore
{
    /**
     * Disclaimer: This code has been inspired and adapted from QuantConnect.Toolbox
     * https://github.com/QuantConnect/Lean/blob/master/LICENSE
     */
    public class FileWriter
    {
        private readonly string _dataDirectory;
        private readonly MarketSymbol _marketSymbol;
        private readonly DataType _dataType;

        public FileWriter(string dataDirectory, MarketSymbol marketSymbol, DataType dataType)
        {
            _dataType = dataType;
            _marketSymbol = marketSymbol;
            _dataDirectory = dataDirectory;
        }

        public void Write(IEnumerable<HistoricalData> historicalData)
        {
            switch (_dataType)
            {
                case DataType.Fundamental:
                    WriteFundamentalOrDaily(historicalData, FundamentalData.Parse);
                    break;
                case DataType.Tick:
                    WriteTick(historicalData);
                    break;
            }
        }

        private void WriteTick(IEnumerable<HistoricalData> historicalData)
        {
            var sb = new StringBuilder();
            var lastTime = new DateTime();

            foreach (var historical in historicalData)
            {
                // Ensure the data is sorted
                if (historical.Timestamp < lastTime) throw new Exception("The data must be pre-sorted from oldest to newest");

                // Based on the security type and resolution, write the data to the zip file
                if (lastTime != DateTime.MinValue && historical.Timestamp.Date > lastTime.Date)
                {
                    // Write and clear the file contents
                    var outputFile = DataStore.GetZipOutputFileName(lastTime, _marketSymbol, _dataType, _dataDirectory);
                    WriteFile(outputFile, sb.ToString(), lastTime);
                    sb.Clear();
                }

                lastTime = historical.Timestamp;

                // Build the line and append it to the file
                sb.Append(historical.Value + Environment.NewLine);
            }

            // Write the last file
            if (sb.Length > 0)
            {
                var outputFile = DataStore.GetZipOutputFileName(lastTime, _marketSymbol, _dataType, _dataDirectory);
                WriteFile(outputFile, sb.ToString(), lastTime);
            }
        }

        private void WriteFundamentalOrDaily(IEnumerable<HistoricalData> source, Func<string, HistoricalData> parseFunc)
        {
            var sb = new StringBuilder();
            var lastTime = new DateTime();

            // Determine file path
            var outputFile = DataStore.GetZipOutputFileName(lastTime, _marketSymbol, _dataType, _dataDirectory);

            // Load new data rows into a SortedDictionary for easy merge/update
            var newRows = new SortedDictionary<DateTime, string>(source.ToDictionary(x => x.Timestamp, x => x.Value));
            SortedDictionary<DateTime, string> rows;

            if (File.Exists(outputFile))
            {
                // If file exists, we load existing data and perform merge
                rows = new SortedDictionary<DateTime, string>(LoadFundamentalOrDaily(outputFile, parseFunc).ToDictionary(x => x.Timestamp, x => x.Value));
                foreach (var kvp in newRows)
                {
                    rows[kvp.Key] = kvp.Value;
                }
            }
            else
            {
                // No existing file, just use the new data
                rows = newRows;
            }

            // Loop through the SortedDictionary and write to file contents
            foreach (var kvp in rows)
            {
                // Build the line and append it to the file
                sb.Append(kvp.Value + Environment.NewLine);
            }

            // Write the file contents
            if (sb.Length > 0)
            {
                WriteFile(outputFile, sb.ToString(), lastTime);
            }
        }

        private static IEnumerable<HistoricalData> LoadFundamentalOrDaily(string fileName, Func<string, HistoricalData> parseFunc)
        {
            using (var zip = ZipFile.Read(fileName))
            {
                using (var stream = new MemoryStream())
                {
                    zip[0].Extract(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            yield return parseFunc(line);
                        }
                    }
                }
            }
        }

        private void WriteFile(string filePath, string data, DateTime date)
        {
            var tempFilePath = filePath + ".tmp";

            data = data.TrimEnd();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Log.Information("FileWriter.Write(): Existing deleted: " + filePath);
            }

            // Create the directory if it doesnt exist
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // Write out this data string to a zip file
            Compression.Zip(data, tempFilePath, DataStore.GenerateZipEntryName(date, _marketSymbol, _dataType));

            // Move temp file to the final destination with the appropriate name
            File.Move(tempFilePath, filePath);

            Log.Information("FileWriter.Write(): Created: " + filePath);
        }
    }
}