using System;
using System.Collections.Generic;
using System.Globalization;

namespace IQFeed.DataStore
{
    public class FundamentalData : HistoricalData
    {
        public FundamentalData(DateTime timestamp, string message) : base(timestamp, $"{timestamp.Date.ToString(DataStore.DateFormat)},{message}")
        {
            Message = message;
        }

        public string Message { get; private set; }

        public static FundamentalData Parse(string line)
        {
            var date = DateTime.ParseExact(line.Substring(0, DataStore.DateFormat.Length), DataStore.DateFormat, CultureInfo.InvariantCulture);
            return new FundamentalData(date, line.Substring(DataStore.DateFormat.Length + 1));
        }

        public static IEnumerable<FundamentalData> GetFundamentals(string filePath)
        {
            using (var file = Compression.Unzip(filePath, null, out var zipFile))
            {
                string line;

                while ((line = file.ReadLine()) != null)
                {
                    yield return FundamentalData.Parse(line);
                }
            }
        }
    }
}