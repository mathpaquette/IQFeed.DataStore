using System;

namespace IQFeed.DataStore
{
    public class HistoricalData
    {
        public DateTime Timestamp { get; }
        public string Line { get; }

        public HistoricalData(DateTime timestamp, string line)
        {
            Timestamp = timestamp;
            Line = line;
        }
    }
}