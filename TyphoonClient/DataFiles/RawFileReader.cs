using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    public class RawFileReader : IDataFileReader
    {
        private readonly IntPtr masslynxRawReader;

        [DllImport("Waters.Library.MassLynxRaw")]
        private static extern IntPtr CreateRawReader(string filename);
        [DllImport("Waters.Library.MassLynxRaw")]
        private static extern void DestroyRawReader(IntPtr masslynxRawReader);
        [DllImport("Waters.Library.MassLynxRaw")]
        private static extern UInt32 RawReaderFunctionCount(IntPtr masslynxRawReader);
        [DllImport("Waters.Library.MassLynxRaw")]
        private static extern UInt32 RawReaderScanCount(IntPtr masslynxRawReader, UInt32 function);
        [DllImport("Waters.Library.MassLynxRaw")]
        private static extern void RawReaderLoadScan(IntPtr masslynxRawReader, UInt32 function, UInt32 scan);
        [DllImport("Waters.Library.MassLynxRaw")]
        private static extern UInt64 RawReaderScanDataSize(IntPtr masslynxRawReader);
        [DllImport("Waters.Library.MassLynxRaw")]
        private static extern bool RawReaderGetScanDataBuffer(IntPtr masslynxRawReader, byte[] serializedScanDataBuffer, int serializedScanDataBufferSize);

        public RawFileReader(string filename)
        {
            masslynxRawReader = CreateRawReader(filename);
        }

        public int FunctionCount()
        {
            return (int)RawReaderFunctionCount(masslynxRawReader);
        }

        public int ScanCount(int function)
        {
            return (int)RawReaderScanCount(masslynxRawReader, (UInt32)function);
        }

        public IEnumerable<ScanData> ReadScans(int function)
        {
            var scanCount = ScanCount(function);
            for (var scan = 0; scan < scanCount; ++scan)
            {
                yield return GetScan(function, scan);
            }
        }

        public IEnumerable<ScanData> ReadScans()
        {
            List<ScanData> scans = new List<ScanData>();
            var functionCount = FunctionCount();
            for (var function = 0; function < functionCount; ++function)
            {
                var scanCount = ScanCount(function);
                for (var scan = 0; scan < scanCount; ++scan)
                {
                    scans.Add(GetScan(function, scan));
                }
            }
            scans.Sort((s1, s2) => s1.Header.RetentionTime.CompareTo(s2.Header.RetentionTime));
            return scans;
        }

        private ScanData GetScan(int function, int scan)
        {
            RawReaderLoadScan(masslynxRawReader, (UInt32)function, (UInt32)scan);
            var size = RawReaderScanDataSize(masslynxRawReader);
            byte[] data = new byte[size];
            RawReaderGetScanDataBuffer(masslynxRawReader, data, data.Length);
            var scanData = MessageSerializer.Deserialize<ScanData>(data);
            if (scanData.Settings.Calibration.Mapping == null)
            {
                // Fix up invalid calibration, if we have no mapping wipe out entire calibration.
                scanData.Settings.Calibration = null;
            }

            return scanData;
        }

        private void ReleaseUnmanagedResources()
        {
            DestroyRawReader(masslynxRawReader);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~RawFileReader()
        {
            ReleaseUnmanagedResources();
        }
    }
}
