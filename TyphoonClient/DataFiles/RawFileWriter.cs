using System;
using System.Runtime.InteropServices;
using Google.Protobuf;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    public class RawFileWriter : IDataFileWriter
    {
        private readonly IntPtr masslynxRawWriter;

        [DllImport("Waters.Library.MassLynxRaw")]
        private static extern IntPtr CreateRawWriter(string filename);
        [DllImport("Waters.Library.MassLynxRaw")]
        private static extern void DestroyRawWriter(IntPtr masslynxRawWriter);
        [DllImport("Waters.Library.MassLynxRaw")]
        private static extern void RawWriterWriteScan(IntPtr masslynxRawWriter, byte[] serializedScanDataBuffer, int serializedScanDataBufferSize);


        public RawFileWriter(string filename)
        {
            masslynxRawWriter = CreateRawWriter(filename);
        }

        public void Append(ScanData scanData)
        {
            byte[] messageData = scanData.ToByteArray();
            RawWriterWriteScan(masslynxRawWriter, messageData, messageData.Length);
        }

        private void ReleaseUnmanagedResources()
        {
            DestroyRawWriter(masslynxRawWriter);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~RawFileWriter()
        {
            ReleaseUnmanagedResources();
        }
    }
}
