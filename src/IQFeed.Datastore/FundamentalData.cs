using System;
using System.Globalization;

namespace IQFeed.DataStore
{
    public class FundamentalData : HistoricalData
    {
        public FundamentalData(DateTime timestamp, string value) : base(timestamp, $"{timestamp.Date.ToString(DataStore.DateFormat)},{value}") { }

        public static FundamentalData Parse(string line)
        {
            var date = DateTime.ParseExact(line.Substring(0, DataStore.DateFormat.Length), DataStore.DateFormat, CultureInfo.InvariantCulture);
            return new FundamentalData(date, line.Substring(DataStore.DateFormat.Length + 1));
        }
    }
}