using System.Collections.Generic;
using IQFeed.CSharpApiClient.Lookup.Symbol.MarketSymbols;

namespace IQFeed.DataStore
{
    public class MarketSymbolReader
    {
        public static IEnumerable<MarketSymbol> GetMarketSymbols(string filePath)
        {
            var lineCount = 0;

            using (var file = Compression.Unzip(filePath, "mktsymbols_v2.txt", out var zipFile))
            {
                string line;
                string[] values;

                while ((line = file.ReadLine()) != null && (values = line.Split('\t')).Length == 8)
                {
                    lineCount++;

                    if (lineCount == 1) // ignore the header
                        continue;

                    yield return new MarketSymbol(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7]);
                }
            }
        }
    }
}