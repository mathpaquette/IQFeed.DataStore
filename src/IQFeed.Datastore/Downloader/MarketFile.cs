using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;

namespace IQFeed.Datastore.Downloader
{
    public class MarketFile
    {
        public MarketSymbol MarketSymbol { get; }
        public string Filename { get; }
        public Resolution Resolution { get; }

        public MarketFile(MarketSymbol marketSymbol, string filename, Resolution resolution)
        {
            MarketSymbol = marketSymbol;
            Filename = filename;
            Resolution = resolution;
        }
    }
}