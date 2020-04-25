using System;
using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;

namespace IQFeed.DataStore.Downloader
{
    public class MarketRequest
    {
        public MarketSymbol MarketSymbol { get; }
        public DataType DataType { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public MarketRequest(MarketSymbol marketSymbol, DataType dataType, DateTime startDate, DateTime endDate)
        {
            MarketSymbol = marketSymbol;
            DataType = dataType;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}