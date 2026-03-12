using System;
using System.Runtime.InteropServices;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    public class ScanDataCombiner : IDisposable
    {
        private readonly IntPtr scanDataCombiner;

        [DllImport("Waters.Library.Maths")]
        private static extern IntPtr CreateScanCombiner();

        [DllImport("Waters.Library.Maths")]
        private static extern void DestroyScanCombiner(IntPtr scanDataCombiner);

        [DllImport("Waters.Library.Maths")]
        private static extern void AddToScanCombiner(IntPtr scanDataCombiner, byte[] serializedScanData, int serializedScanDataSize);

        [DllImport("Waters.Library.Maths")]
        private static extern int GetScanCombinerScanCount(IntPtr scanDataCombiner);

        [DllImport("Waters.Library.Maths")]
        private static extern int GetScanCombinerSize(IntPtr scanDataCombiner);

        [DllImport("Waters.Library.Maths")]
        private static extern void GetScanCombinerData(IntPtr scanDataCombiner, byte[] serializedScanDataBuffer, int serializedScanDataBufferSize);

        public int ScanCount => GetScanCombinerScanCount(scanDataCombiner);

        public ScanDataCombiner()
        {
            scanDataCombiner = CreateScanCombiner();
        }

        public void Add(ScanData scanData)
        {
            byte[] messageData = MessageSerializer.Serialize(scanData);
            AddToScanCombiner(scanDataCombiner, messageData, messageData.Length);
        }


        public ScanData Get()
        {
            // This approach is quite inefficient since we combine and serialize the scan data twice, once to get the data length then
            // a second time to get the data, we can replace the C++ side with a wrapper than caches the result if needed for performance
            var size = GetScanCombinerSize(scanDataCombiner);
            byte[] data = new byte[size];
            GetScanCombinerData(scanDataCombiner, data, data.Length);
            return MessageSerializer.Deserialize<ScanData>(data);
        }

        private void ReleaseUnmanagedResources()
        {
            DestroyScanCombiner(scanDataCombiner);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~ScanDataCombiner()
        {
            ReleaseUnmanagedResources();
        }
    }
}
