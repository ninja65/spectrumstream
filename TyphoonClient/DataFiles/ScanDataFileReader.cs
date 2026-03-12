using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;
using SharpCompress.Compressors.LZMA;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    public class ScanDataFileReader : IDataFileReader
    {
        private readonly string filename;
        private BinaryReader reader;
        private Stream fileStream;
        private Stream compressionStream;

        private static readonly ScanDataHeaderBlock DefaultHeaderBlock = new ScanDataHeaderBlock();

        public ScanDataFileReader(string filename)
        {
            this.filename = filename;
        }

        public static bool IsValid(string filename)
        {
            if (!File.Exists(filename))
            {
                return false;
            }

            try
            {
                using var reader = new ScanDataFileReader(filename);
                reader.LoadHeader();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        public IEnumerable<ScanData> ReadScans()
        {
            LoadHeader();

            while (true)
            {
                byte[] bytes;
                try
                {
                    var size = reader.ReadInt32();
                    bytes = reader.ReadBytes(size);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
                yield return ScanData.Parser.ParseFrom(bytes);
            }
        }

        public void Dispose()
        {
            reader?.Dispose();
            reader = null;
            compressionStream?.Dispose();
            compressionStream = null;
            fileStream?.Dispose();
            fileStream = null;
        }

        private void LoadHeader()
        {
            fileStream = File.Open(filename, FileMode.Open);
            reader = new BinaryReader(fileStream, Encoding.Default, true);

            var headerBlock = new ScanDataHeaderBlock
            {
                Magic = reader.ReadBytes(2),
                Version = reader.ReadByte(),
                Type = reader.ReadByte()
            };

            if (headerBlock.Magic[0] != DefaultHeaderBlock.Magic[0] || headerBlock.Magic[1] != DefaultHeaderBlock.Magic[1])
            {
                throw new InvalidDataException("Not a valid scan data file");
            }

            if (headerBlock.Version != 1)
            {
                throw new InvalidDataException($"Unsupported scan data file version: {headerBlock.Version}");
            }

            switch ((ScanDataFileType)headerBlock.Type)
            {
                case ScanDataFileType.Uncompressed:
                    break;
                case ScanDataFileType.GzipCompressed:
                    compressionStream = new GZipStream(fileStream, CompressionMode.Decompress);
                    reader = new BinaryReader(compressionStream, Encoding.Default, true);
                    break;
                case ScanDataFileType.BZip2Compressed:
                    compressionStream = new BZip2InputStream(fileStream);
                    reader = new BinaryReader(compressionStream, Encoding.Default, true);
                    break;
                case ScanDataFileType.LZipCompressed:
                    compressionStream = new LZipStream(fileStream, SharpCompress.Compressors.CompressionMode.Decompress);
                    reader = new BinaryReader(compressionStream, Encoding.Default, true);
                    break;
                default:
                    throw new InvalidDataException($"Unsupported scan data file type: {headerBlock.Type}");
            }
        }
    }
}