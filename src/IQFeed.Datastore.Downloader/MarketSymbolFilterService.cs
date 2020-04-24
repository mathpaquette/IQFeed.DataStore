using System.Collections.Generic;
using System.Linq;

namespace IQFeed.Datastore.Downloader
{
    public class MarketSymbolFilterService
    {
        private readonly List<FilterDesc> _filters;

        public MarketSymbolFilterService()
        {
            _filters = new List<FilterDesc>();
            Initialize();
        }

        private void Initialize()
        {
            _filters.Add(new FilterDesc("US_STOCKS", symbols =>
            {
                var exchanges = new List<string>() { "NYSE", "NASDAQ", "NYSE_AMERICAN", "BATS", "IEX" };
                return symbols.Where(x => x.SecurityType == "EQUITY" && exchanges.Contains(x.Exchange) && x.ListedMarket != "OTC");
            }, ""));

            _filters.Add(new FilterDesc("US_STOCKS_WITH_OTC", symbols =>
            {
                var exchanges = new List<string>() { "NYSE", "NASDAQ", "NYSE_AMERICAN", "BATS", "IEX" };
                return symbols.Where(x => x.SecurityType == "EQUITY" && exchanges.Contains(x.Exchange) && x.ListedMarket != "OTC");
            }, ""));
        }

        public FilterDesc GetFilterDescByGroupId(string id)
        {
            return _filters.FirstOrDefault(x => x.Id == id);
        }
    }
}