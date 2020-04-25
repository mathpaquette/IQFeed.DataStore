using System;
using System.IO;
using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;
using IQFeed.DataStore.Downloader;

namespace IQFeed.DataStore
{
    /**
     * Disclaimer: This code has been inspired and adapted from QuantConnect.Toolbox
     * https://github.com/QuantConnect/Lean/blob/master/LICENSE
     */
    public class DataStore
    {
        public const string DateFormat = "yyyyMMdd";

        public static string GenerateZipEntryName(DateTime date, MarketSymbol symbol, DataType dataType)
        {
            var formattedSymbol = symbol.Symbol.ToLowerInvariant();
            var formattedDataType = dataType.ToString().ToLowerInvariant();

            switch (dataType)
            {
                case DataType.Fundamental:
                case DataType.Daily:
                case DataType.Weekly:
                    return $"{formattedSymbol}_{formattedDataType}.csv";
                default:
                    return $"{date.ToString(DateFormat)}_{formattedSymbol}_{formattedDataType}.csv";
            }
        }

        public static string GetZipOutputFileName(DateTime date, MarketSymbol symbol, DataType dataType, string dataDirectory)
        {
            var formattedSecurityType = symbol.SecurityType.ToLowerInvariant();
            var formattedDataType = dataType.ToString().ToLowerInvariant();
            var formattedSymbol = symbol.Symbol.ToLowerInvariant();
            var country = "usa"; // TODO: find country by exchange

            return Path.Combine(dataDirectory, formattedSecurityType, country, formattedDataType, formattedSymbol, GenerateZipFileName(date, symbol, dataType));
        }

        public static string GenerateZipFileName(DateTime date, MarketSymbol symbol,  DataType dataType)
        {
            var formattedSymbol = symbol.Symbol.ToLowerInvariant();

            switch (dataType)
            {
                case DataType.Fundamental:
                case DataType.Daily:
                case DataType.Weekly:
                    return $"{formattedSymbol}.zip";
                default:
                    return $"{date:yyyyMMdd}_{formattedSymbol}.zip"; ;
            }
        }
    }
}