using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;

namespace IQFeed.Datastore.Downloader
{
    public class MarketRequest
    {
        public MarketSymbol MarketSymbol { get; }
        public Resolution Resolution { get; }

        public MarketRequest(MarketSymbol marketSymbol, Resolution resolution)
        {
            MarketSymbol = marketSymbol;
            Resolution = resolution;
        }
    }
}