using System;
using System.Collections.Generic;
using System.Linq;
using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;

namespace IQFeed.DataStore
{
    public static class MarketSymbolExtensions
    {
        public static IEnumerable<MarketSymbol> FilterBy(this IEnumerable<MarketSymbol> marketSymbols, string id)
        {
            return FiltersById.TryGetValue(id, out var filter) ?
                filter(marketSymbols) : Enumerable.Empty<MarketSymbol>();
        }

        private static Dictionary<string, Func<IEnumerable<MarketSymbol>, IEnumerable<MarketSymbol>>> FiltersById = new Dictionary<string, Func<IEnumerable<MarketSymbol>, IEnumerable<MarketSymbol>>>()
        {
            { "US_STOCKS",
                symbols => {
                var exchanges = new List<string>() { "NYSE", "NASDAQ", "NYSE_AMERICAN", "BATS", "IEX" };
                return symbols.Where(x => x.SecurityType == "EQUITY" && exchanges.Contains(x.Exchange) && x.ListedMarket != "OTC");
            }}
        };
    }
}