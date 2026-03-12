using System.Collections.Generic;
using System.Runtime.InteropServices;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    public class ScanDataConverter
    {
        [DllImport("Waters.Library.Maths")]
        private static extern bool ScanDataToMassIntensity(byte[] serializedScanData, int serializedScanDataSize, double[] masses, double[] intensities, int dataPoints);

        public static Spectrum Convert(ScanData scanData)
        {
            var spectrum = new Spectrum { RetentionTime = scanData.Header.RetentionTime };
            if (scanData.Channel.Count <= 0)
            {
                return spectrum;
            }

            byte[] messageData = MessageSerializer.Serialize(scanData);
            var dataPoints = scanData.Channel[0].MassIndex.Count;
            var masses = new double[dataPoints];
            var intensities = new double[dataPoints];

            if (!ScanDataToMassIntensity(messageData, messageData.Length, masses, intensities, dataPoints))
            {
                return spectrum;
            }

            spectrum.MassList = new List<DataPoint>(dataPoints);
            for (int i = 0; i < dataPoints; ++i)
            {
                spectrum.MassList.Add(new DataPoint { Mass = masses[i], Intensity = intensities[i] });
            }

            return spectrum;
        }
    }
}
