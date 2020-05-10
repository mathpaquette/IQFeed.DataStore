using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using IQFeed.DataStore.Downloader;

namespace IQFeed.DataStore
{
    public class FileArchive
    {
        private const string Pattern = @"([^\\]*?)\\([^\\]*?)\\([^\\]*?)\\([^\\]*?)\\([^\\]*?).zip$";

        public FileArchive(string securityType, DataType dataType, string symbol, string path, DateTime date, string country)
        {
            SecurityType = securityType;
            DataType = dataType;
            Symbol = symbol;
            Path = path;
            Date = date;
            Country = country;
        }

        public string SecurityType { get; }
        public DataType DataType { get; }
        public string Symbol { get; }
        public DateTime Date { get; }
        public string Path { get; }
        public string Country { get; }


        public static IEnumerable<FileArchive> GetFileArchives(string dataDirectory)
        {
            var files = Directory.GetFiles(dataDirectory, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var m = Regex.Matches(file, Pattern);
                if(m.Count == 0)
                    continue;
                
                var groups = m[0].Groups;
                
                
                var securityType = groups[1].Value.ToUpperInvariant();
                var country = groups[2].Value.ToUpperInvariant();
                DataType dataType;
                Enum.TryParse(groups[3].Value,true, out dataType);
                var name = groups[4].Value.ToUpperInvariant();
                var date = dataType == DataType.Tick ? DateTime.ParseExact(groups[5].Value, DataStore.DateFormat, CultureInfo.InvariantCulture) : DateTime.MinValue;

                yield return new FileArchive(securityType, dataType, name, file, date, country);
            }
        }

        public override string ToString()
        {
            return $"{nameof(SecurityType)}: {SecurityType}, {nameof(DataType)}: {DataType}, {nameof(Symbol)}: {Symbol}, {nameof(Date)}: {Date}, {nameof(Path)}: {Path}, {nameof(Country)}: {Country}";
        }
    }
}