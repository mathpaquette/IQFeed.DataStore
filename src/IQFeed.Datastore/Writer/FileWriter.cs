using System;
using System.IO;
using System.Text;
using IQFeed.CSharpApiClient.Lookup.Common;
using IQFeed.CSharpApiClient.Lookup.Historical.Messages;
using IQFeed.Datastore.Downloader;

namespace IQFeed.Datastore.Writer
{
    public class FileWriter
    {
        private readonly string _dataDirectory;

        public FileWriter(string dataDirectory)
        {
            _dataDirectory = dataDirectory;
        }

        public void Write(MarketFile marketFile)
        {
            switch (marketFile.Resolution)
            {
                case Resolution.Tick:
                    WriteTick(marketFile);
                    break;
            }
        }

        private void WriteTick(MarketFile marketFile)
        {
            var sb = new StringBuilder();
            var lastTime = new DateTime();
            var lines = LookupMessageFileParser.ParseFromFile(line => new { Line = line, Timestamp = TickMessage.Parse(line).Timestamp }, marketFile.Filename);

            foreach (var line in lines)
            {
                // Ensure the data is sorted
                if (line.Timestamp < lastTime) throw new Exception("The data must be pre-sorted from oldest to newest");

                // Based on the security type and resolution, write the data to the zip file
                if (lastTime != DateTime.MinValue && line.Timestamp.Date > lastTime.Date)
                {
                    // Write and clear the file contents
                    var outputFile = GetZipOutputFileName(marketFile, lastTime);
                    WriteFile(outputFile, sb.ToString(), lastTime, marketFile);
                    sb.Clear();
                }

                lastTime = line.Timestamp;

                // Build the line and append it to the file
                sb.Append(line.Line + Environment.NewLine);
            }

            // Write the last file
            if (sb.Length > 0)
            {
                var outputFile = GetZipOutputFileName(marketFile, lastTime);
                WriteFile(outputFile, sb.ToString(), lastTime, marketFile);
            }
        }

        private void WriteFile(string filePath, string data, DateTime date, MarketFile marketFile)
        {
            var tempFilePath = filePath + ".tmp";

            data = data.TrimEnd();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                // Log.Trace("LeanDataWriter.Write(): Existing deleted: " + filePath);
            }

            // Create the directory if it doesnt exist
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // Write out this data string to a zip file
            Compression.Zip(data, tempFilePath, GenerateZipEntryName(date, marketFile));

            // Move temp file to the final destination with the appropriate name
            File.Move(tempFilePath, filePath);

            // Log.Trace("LeanDataWriter.Write(): Created: " + filePath);
        }

        private string GenerateZipEntryName(DateTime date, MarketFile marketFile)
        {
            var symbol = marketFile.MarketSymbol.Symbol.ToLowerInvariant();
            var dataType = "Trade";
            var resolution = marketFile.Resolution.ToString();
            
            return $"{date:yyyyMMdd}_{symbol}_{dataType}_{resolution}.csv";
        }

        private string GetZipOutputFileName(MarketFile marketFile, DateTime date)
        {
            var securityType = marketFile.MarketSymbol.SecurityType.ToLowerInvariant();
            var country = "usa";
            var resolution = marketFile.Resolution.ToString().ToLowerInvariant();
            var symbol = marketFile.MarketSymbol.Symbol.ToLowerInvariant();
            var filename = $"{date:yyyyMMdd}_trade.zip";

            return Path.Combine(_dataDirectory, securityType, country, resolution, symbol, filename);
        }
    }
}