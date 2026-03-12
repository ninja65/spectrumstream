using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Google.Protobuf;
using ICSharpCode.SharpZipLib.BZip2;
using SharpCompress.Compressors.LZMA;
using Waters.Control.Message;
using CompressionMode = SharpCompress.Compressors.CompressionMode;

namespace Waters.Control.Client
{
    public class ScanDataFileWriter : IDataFileWriter
    {
        private const ScanDataFileType DefaultFileType = ScanDataFileType.BZip2Compressed;
        private BinaryWriter writer;
        private Stream fileStream;
        private Stream compressionStream;
        private static readonly ScanDataHeaderBlock DefaultHeaderBlock = new ScanDataHeaderBlock();

        public ScanDataFileWriter(string filename) : this(filename, DefaultFileType)
        {
        }

        public ScanDataFileWriter(string filename, ScanDataFileType type)
        {
            filename = Path.GetFullPath(filename);
            Directory.CreateDirectory(Path.GetDirectoryName(filename));

            fileStream = File.Open(filename, FileMode.Create);
            writer = new BinaryWriter(fileStream, Encoding.Default, true);
            WriteHeader(type);

            switch (type)
            {
                case ScanDataFileType.Uncompressed:
                    break;
                case ScanDataFileType.GzipCompressed:
                    compressionStream = new GZipStream(fileStream, CompressionLevel.Fastest);
                    writer.Close();
                    writer = new BinaryWriter(compressionStream, Encoding.Default, true);
                    break;
                case ScanDataFileType.BZip2Compressed:
                    compressionStream = new BZip2OutputStream(fileStream);
                    writer.Close();
                    writer = new BinaryWriter(compressionStream, Encoding.Default, true);
                    break;
                case ScanDataFileType.LZipCompressed:
                    compressionStream = new LZipStream(fileStream, CompressionMode.Compress);
                    writer.Close();
                    writer = new BinaryWriter(compressionStream, Encoding.Default, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void Append(ScanData scanData)
        {
            var bytes = scanData.ToByteArray();
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        public void Dispose()
        {
            writer?.Dispose();
            compressionStream?.Dispose();
            fileStream?.Dispose();
            writer = null;
        }

        private void WriteHeader(ScanDataFileType type)
        {
            writer.Write(DefaultHeaderBlock.Magic);
            writer.Write(DefaultHeaderBlock.Version);
            writer.Write((byte)type);
        }
    }
}