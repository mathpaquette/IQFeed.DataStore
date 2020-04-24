using System;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using ZipEntry = ICSharpCode.SharpZipLib.Zip.ZipEntry;
using ZipFile = Ionic.Zip.ZipFile;
using ZipOutputStream = ICSharpCode.SharpZipLib.Zip.ZipOutputStream;

namespace IQFeed.Datastore
{
    /**
     * Disclaimer: This code has been inspired and adapted from QuantConnect.Toolbox
     */
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

        /// <summary>
        /// Streams a local zip file using a streamreader.
        /// Important: the caller must call Dispose() on the returned ZipFile instance.
        /// </summary>
        /// <param name="filename">Location of the original zip file</param>
        /// <param name="zipEntryName">The zip entry name to open a reader for. Specify null to access the first entry</param>
        /// <param name="zip">The ZipFile instance to be returned to the caller</param>
        /// <returns>Stream reader of the first file contents in the zip file</returns>
        public static StreamReader Unzip(string filename, string zipEntryName, out ZipFile zip)
        {
            StreamReader reader = null;
            zip = null;

            try
            {
                if (File.Exists(filename))
                {
                    try
                    {
                        zip = new ZipFile(filename);
                        var entry = zip.FirstOrDefault(x => zipEntryName == null || string.Compare(x.FileName, zipEntryName, StringComparison.OrdinalIgnoreCase) == 0);

                        if (entry == null)
                        {
                            // Unable to locate zip entry
                            return null;
                        }

                        reader = new StreamReader(entry.OpenReader());
                    }
                    catch (Exception err)
                    {
                        Log.Error(err, "Inner try/catch");
                        if (zip != null) zip.Dispose();
                        if (reader != null) reader.Close();
                    }
                }
                else
                {
                    Log.Error($"Data.UnZip(2): File doesn\'t exist: {filename}");
                }
            }
            catch (Exception err)
            {
                Log.Error(err, "File: " + filename);
            }
            return reader;
        }
    }
}