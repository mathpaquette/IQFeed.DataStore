using System;
using System.Collections.Generic;
using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;

namespace IQFeed.Datastore.Downloader
{
    public class FilterDesc
    {
        public string Id { get; }
        public Func<IEnumerable<MarketSymbol>, IEnumerable<MarketSymbol>> Func { get; }
        public string Description { get; }

        public FilterDesc(string id, Func<IEnumerable<MarketSymbol>, IEnumerable<MarketSymbol>> func, string description)
        {
            Id = id;
            Func = func;
            Description = description;
        }
    }
}