using System;
using System.Globalization;

namespace IQFeed.Datastore
{
    public class FundamentalData : HistoricalData
    {
        public const string DateFormat = "yyyyMMdd";

        public FundamentalData(DateTime timestamp, string value) : base(timestamp, $"{timestamp.Date.ToString(DateFormat)},{value}") { }

        public static FundamentalData Parse(string line)
        {
            var date = DateTime.ParseExact(line.Substring(0, DateFormat.Length), DateFormat, CultureInfo.InvariantCulture);
            return new FundamentalData(date, line.Substring(DateFormat.Length + 1));
        }
    }
}