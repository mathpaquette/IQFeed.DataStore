using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace IQFeed.Datastore.Writer
{
    public static class Compression
    {
        public static void Zip(string data, string zipPath, string zipEntry)
        {
            using (var stream = new ZipOutputStream(File.Create(zipPath)))
            {
                var entry = new ZipEntry(zipEntry);
                stream.PutNextEntry(entry);
                var buffer = new byte[4096];
                using (var dataReader = new MemoryStream(Encoding.Default.GetBytes(data)))
                {
                    int sourceBytes;
                    do
                    {
                        sourceBytes = dataReader.Read(buffer, 0, buffer.Length);
                        stream.Write(buffer, 0, sourceBytes);
                    }
                    while (sourceBytes > 0);
                }
            }
        }
    }
}