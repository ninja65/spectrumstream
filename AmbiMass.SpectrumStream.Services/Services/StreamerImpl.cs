using AmbiMass.SpectrumStream.Contracts.Interfaces;
using AmbiMass.SpectrumStream.Data.RealtimeData;
using AmbiMass.SpectrumStream.Utils.SysEnv;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly ISysEnvironment _sysEnvironment;
        private readonly ILogger _logger;

        // Keep each chunk comfortably below typical message-size limits.
        // 16 KB payload is conservative.
        private const int DpSize = 16;

        private const int ChunkSizeBytes = DpSize * 1024;

        public StreamerImpl( ISignalRHub signalRHub, ISysEnvironment sysEnvironment,
            ILogger<StreamerImpl> logger ) 
        {
            _signalRHub = signalRHub;

            _sysEnvironment = sysEnvironment;

            _logger = logger;
        }
        internal void streamOutSpectrum( int scanNumber, Spectrum scanData)
        {
            // Serialize [all X doubles][all Y doubles] as little-endian bytes
            try
            {
                byte[] payload = serializeTwoDoubleArrays( scanData.MassList.ToArray() );

                int totalChunks = (payload.Length + ChunkSizeBytes - 1) / ChunkSizeBytes;

                var scanStarted = new ScanStarted()
                {
                    scanId = scanNumber,
                    count = scanData.MassList.Count,
                    totalBytes = payload.Length,
                    totalChunks = totalChunks,
                    chunkSize = ChunkSizeBytes
                };

                _signalRHub.scanStarted( scanStarted );

                int totalBytes = 0;

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
                            count = len,
                            data = chunk,
                            offset = offset
                        });

                        totalBytes += len;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(chunk, clearArray: false);
                    }
                
                }
            
                _signalRHub.scanCompleted(new ScanCompleted()
                {
                    scanId = scanNumber,

                    timeStamp = _sysEnvironment.getCurrentTimeUtc().ToString("mm:ss.fff")
                });
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error in streamOutput");
            }
        }

        internal unsafe byte[] serializeTwoDoubleArrays( DataPoint[] dataPoints )
        {
            if (dataPoints is null) throw new ArgumentNullException(nameof(dataPoints));

            byte[] buffer = new byte[dataPoints.Length * DpSize];

            fixed (byte* targetBegin = buffer) 
            {
                double* target = (double*) targetBegin;

                fixed( DataPoint* sourceBegin = dataPoints)
                {
                    DataPoint* sourcePtr = (DataPoint*) sourceBegin;

                    DataPoint* sourceEnd = sourceBegin + dataPoints.Length;

                    while( sourcePtr <  sourceEnd ) 
                    {
                        *target = sourcePtr->Mass;

                        target++;

                        *target = sourcePtr->Intensity;

                        target++;    

                        sourcePtr++;
                    }
                }
            }

            return buffer;
        }
    }
}
