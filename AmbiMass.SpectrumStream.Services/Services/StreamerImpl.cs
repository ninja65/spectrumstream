using AmbiMass.SpectrumStream.Contracts.Interfaces;
using AmbiMass.SpectrumStream.Data.RealtimeData;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waters.Control.Client;
using Windows.Management.Update;

namespace AmbiMass.SpectrumStream.Services.Services
{
    public class StreamerImpl
    {
        private readonly ISignalRHub _signalRHub;

        // Keep each chunk comfortably below typical message-size limits.
        // 16 KB payload is conservative.
        private const int ChunkSizeBytes = 24 * 1024;

        public StreamerImpl( ISignalRHub signalRHub) 
        {
            _signalRHub = signalRHub;
        }
        internal void streamOutSpectrum( int scanNumber, Spectrum scanData)
        {
            // Serialize [all X doubles][all Y doubles] as little-endian bytes
            byte[] payload = serializeTwoDoubleArrays( scanData.MassList.ToArray() );

            int totalChunks = (payload.Length + ChunkSizeBytes - 1) / ChunkSizeBytes;

            _signalRHub.scanStarted( new ScanStarted
                {
                    scanId = scanNumber,
                    count = scanData.MassList.Count,
                    totalBytes = payload.Length,
                    totalChunks = totalChunks,
                    chunkSize = ChunkSizeBytes
                });

            for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                int offset = chunkIndex * ChunkSizeBytes;
                int len = Math.Min(ChunkSizeBytes, payload.Length - offset);

                byte[] chunk = ArrayPool<byte>.Shared.Rent(len);

                try
                {
                    Buffer.BlockCopy(payload, offset, chunk, 0, len);

                    _signalRHub.scanChunk(new ScanChunk()
                    {
                        scanId = scanNumber,
                        chunkIndex = chunkIndex,
                        totalChunks = totalChunks,
                        chunkLength = len,
                        itemsInChunk = len / 24,
                        data = chunk
                    });
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(chunk, clearArray: false);
                }
            }

            _signalRHub.scanCompleted(new ScanCompleted()
            {
                scanId = scanNumber,
            });
        }

        internal unsafe byte[] serializeTwoDoubleArrays( DataPoint[] dataPoints )
        {
            if (dataPoints is null) throw new ArgumentNullException(nameof(dataPoints));

            byte[] buffer = new byte[dataPoints.Length * sizeof(DataPoint)];

            Buffer.BlockCopy(dataPoints, 0, buffer, 0, dataPoints.Length * sizeof(DataPoint));

            return buffer;
        }
    }
}
