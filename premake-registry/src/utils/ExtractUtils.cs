
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace src.utils
{
    internal class ExtractUtils
    {
        public static MemoryStream? ReadFile(string sourcePath, string filePath)
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return ReadZipFile(sourcePath, filePath);
            return ReadTarGzFile(sourcePath, filePath);
        }
        private static MemoryStream? ReadTarGzFile(string sourcePath, string filePath)
        {
            using (FileStream fileStream = File.OpenRead(sourcePath))
            using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            using (TarReader tarReader = new TarReader(gzipStream))
            {
                TarEntry? entry;

                while ((entry = tarReader.GetNextEntry()) != null)
                {
                    if (entry.EntryType != TarEntryType.RegularFile || !string.Equals(entry.Name, filePath, StringComparison.Ordinal))
                        continue;
                    

                    MemoryStream memoryStream = new MemoryStream();

                    using (Stream? entryStream = entry.DataStream)
                    {
                        if (entryStream == null)
                            return null;
                        
                        entryStream.CopyTo(memoryStream);
                    }

                    memoryStream.Position = 0;
                    return memoryStream;
                }
            }
            return null;
        }


        private static MemoryStream? ReadZipFile(string sourcePath, string filePath)
        {
            if (!Path.Exists(sourcePath))
                return null;

            using (ZipArchive archive = ZipFile.OpenRead(sourcePath))
            {
                ZipArchiveEntry? entry = archive.GetEntry(filePath);

                if (entry == null)
                    return null;
                MemoryStream memoryStream = new MemoryStream();

                using (Stream entryStream = entry.Open())
                {
                    entryStream.CopyTo(memoryStream);
                }

                memoryStream.Position = 0;
                return memoryStream;
            }
        }

    }
}
