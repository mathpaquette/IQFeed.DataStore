using System;
using System.Collections.Generic;
using Ionic.Zip;
using IQFeed.CSharpApiClient.Lookup.Historical.Messages;
using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;
using IQFeed.Datastore.Downloader;

namespace IQFeed.Datastore
{
    /**
     * Disclaimer: This code has been inspired and adapted from QuantConnect.Toolbox
     * https://github.com/QuantConnect/Lean/blob/master/LICENSE
     */
    public class FileReader
    {
        private readonly string _zipPath;
        private readonly string _zipEntry;

        public FileReader(string dataDirectory, MarketSymbol symbol, DateTime date, DataType resolution)
        {
            _zipPath = DataStore.GetZipOutputFileName(date, symbol, resolution, dataDirectory);
            _zipEntry = DataStore.GenerateZipEntryName(date, symbol, resolution);
        }

        public IEnumerable<IHistoricalMessage> Parse()
        {
            ZipFile zipFile;
            using (var unzipped = Compression.Unzip(_zipPath, _zipEntry, out zipFile))
            {
                if (unzipped == null)
                    yield break;
                string line;
                while ((line = unzipped.ReadLine()) != null)
                {
                    yield return TickMessage.Parse(line);
                }
            }
            zipFile.Dispose();
        }
    }
}