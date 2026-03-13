using AmbiMass.SpectrumStream.Contracts.Interfaces;
using AmbiMass.SpectrumStream.Data.RealtimeData;
using AmbiMass.SpectrumStream.Services.TyphoonSupport;
using Microsoft.Extensions.Logging;
using TyphoonClient.TyphoonClient;
using Waters.Control.Client;
using Waters.Control.Client.Interface;
using Waters.Control.Message;

namespace AmbiMass.SpectrumStream.Services.Services
{
    public class AcquisitionStarterImpl
    {
        private TyphoonSystem? _typhoonSystem;
        private IHardwareControl? _hardwareControl;
        private IJSONLoader _jsonLoader;
        private bool _streaming;
        private readonly ILogger _logger;
        private readonly StreamerImpl _streamer;
        private readonly ISignalRHub _signalRHub;

        public AcquisitionStarterImpl( IJSONLoader jsonLoader, 
            ILogger<AcquisitionStarterImpl> logger,
            StreamerImpl streamer,
            ISignalRHub signalRHub )
        {
            _jsonLoader = jsonLoader;

            _logger = logger;

            _streamer = streamer;

            _signalRHub = signalRHub;
        }
        public void startAcquisition(AcquisitionCommand acquisitionCommand)
        {
            if( acquisitionCommand is null || string.IsNullOrWhiteSpace( acquisitionCommand.MSSettingsFile ) )
            {
                _logger.LogInformation("Acquisitioncommand is null or mssettingsfile is not specified");

                return;
            }

            var settings = _jsonLoader.loadFromFile<MSSettings>( acquisitionCommand.MSSettingsFile );

            _typhoonSystem?.startAcquisition( settings );

            _streaming = true;
        }

        public void stopAcquisition()
        {
            _typhoonSystem?.stopAcquisition();
        }
        public void onTyphoonConnected(TyphoonSystem? typhoonSystem, IHardwareControl? hardwareControl)
        {
            _typhoonSystem = typhoonSystem;

            _hardwareControl = hardwareControl;

            if( typhoonSystem is not null )
            {
                _typhoonSystem.ScanDataEvent += _typhoonSystem_ScanDataEvent;

                _typhoonSystem.Complete += _typhoonSystem_Complete;
            }
        }

        internal void _typhoonSystem_Complete(Acquisition.Result obj)
        {
            //todo: implement
            _streaming = false;
        }

        internal void _typhoonSystem_ScanDataEvent(ScanData obj)
        {
            try
            {
                var scanData = ScanDataConverter.Convert( obj );

                //if( scanData.MassList.Count == 0 )
                //{
                //    scanData.MassList = createRandomMasses();
                //}

                if( scanData.MassList.Count == 0 )
                {
                    return;
                }

                _streamer.streamOutSpectrum( ( int )obj.Header.ScanNumber, scanData );
            }
            catch (Exception ex) {
                _signalRHub.scanFailed(new ScanFailed()
                {
                    scanId = (int)obj.Header.ScanNumber,
                    error = ex.Message,
                });
            }
        }

        internal List<DataPoint> createRandomMasses()
        {
            int numDataPoints = Random.Shared.Next(10000, 350000);

            double startMass = 50.0;
            double endMass = 1100.0;

            double dist = (endMass - startMass) / (numDataPoints - 1);

            DataPoint[] dataPoints = new DataPoint[numDataPoints];


            for( int i = 0; i < numDataPoints; i++ ) {
                dataPoints[i] = new DataPoint();

                dataPoints[i].Mass = startMass;

                dataPoints[i].Intensity = Random.Shared.NextDouble() * 100000.0;

                dataPoints[i].PpmError = 0;

                startMass += dist;
            }

            return dataPoints.ToList();
        }
    }
}
