using AmbiMass.SpectrumStream.Contracts.Interfaces;
using AmbiMass.SpectrumStream.Data.RealtimeData;
using AmbiMass.SpectrumStream.Services.TyphoonSupport;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public AcquisitionStarterImpl( IJSONLoader jsonLoader, 
            ILogger<AcquisitionStarterImpl> logger,
            StreamerImpl streamer )
        {
            _jsonLoader = jsonLoader;

            _logger = logger;

            _streamer = streamer;
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
            var scanData = ScanDataConverter.Convert( obj );

            _streamer.streamOutSpectrum( ( int )obj.Header.ScanNumber, scanData );
        }
    }
}
