using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    public class PeakDetect : IDisposable
    {
        private readonly IntPtr peakDetect;

        [DllImport("Waters.Library.Maths")]
        private static extern IntPtr CreatePeakDetect(byte[] serializedScanData, int serializedScanDataSize);

        [DllImport("Waters.Library.Maths")]
        private static extern void DestroyPeakDetect(IntPtr peakDetect);

        [DllImport("Waters.Library.Maths")]
        private static extern void SetPeakDetectInternalLockMass(IntPtr peakDetect, int lockMassCount, double[] lockMasses, double tolerance);

        [DllImport("Waters.Library.Maths")]
        private static extern void AdaptiveBackgroundSubtract(IntPtr peakDetect);

        [DllImport("Waters.Library.Maths")]
        private static extern int GetPeakDetectEntryCount(IntPtr peakDetect);

        [DllImport("Waters.Library.Maths")]
        private static extern bool GetPeakDetectData(IntPtr peakDetect, int bufferSize, double[] masses, double[] intensities);


        public PeakDetect(ScanData scanData)
        {
            byte[] messageData = MessageSerializer.Serialize(scanData);
            peakDetect = CreatePeakDetect(messageData, messageData.Length);
        }

        public void InternalLockMass(double lockMass, double tolerance)
        {
            // Single point lock mass
            InternalLockMass(new[] { lockMass }, tolerance);
        }

        public void InternalLockMass(double[] lockMasses, double tolerance)
        {
            SetPeakDetectInternalLockMass(peakDetect, lockMasses.Length, lockMasses, tolerance);
        }

        public void AdaptiveBackgroundSubtract()
        {
            AdaptiveBackgroundSubtract(peakDetect);
        }

        public List<DataPoint> DetectPeaks()
        {
            var count = GetPeakDetectEntryCount(peakDetect);
            var masses = new double[count];
            var intensities = new double[count];
            GetPeakDetectData(peakDetect, count, masses, intensities);
            var massList = new List<DataPoint>(count);

            for (var i = 0; i < count; ++i)
            {
                massList.Add(new DataPoint { Mass = masses[i], Intensity = intensities[i] });
            }

            return massList;
        }

        private void ReleaseUnmanagedResources()
        {
            DestroyPeakDetect(peakDetect);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~PeakDetect()
        {
            ReleaseUnmanagedResources();
        }
    }
}
