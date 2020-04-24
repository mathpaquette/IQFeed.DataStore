using System;

namespace IQFeed.Datastore
{
    public class HistoricalData
    {
        public DateTime Timestamp { get; }
        public string Value { get; }

        public HistoricalData(DateTime timestamp, string value)
        {
            Timestamp = timestamp;
            Value = value;
        }
    }
}