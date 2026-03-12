
using AmbiMass.SpectrumStream.Contracts.Interfaces;
using AmbiMass.SpectrumStream.Data.Config;
using AmbiMass.SpectrumStream.Services.TyphoonSupport;
using AmbiMass.SpectrumStream.Utils.CmdLine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TyphoonClient.APIs;
using Waters.Control.Client;
using Waters.Control.Client.Interface;

namespace AmbiMass.SpectrumStream.Services.Services
{
    public class RdaService : BackgroundService
    {
        private TyphoonSystem? _TyphoonSystem;

        private OnlineStatus _TyphoonStatus = OnlineStatus.Offline;

        private IHostApplicationLifetime? _lifetime;

        private volatile bool _typhoonSetupOk = false;

        private TimeSpan  _waitForTyphoonOnlineTimeout;

        private readonly SpectrumStreamSettings _config;

        private readonly ILogger  _logger;

        private readonly ISysEnvironment _sysEnvironment;

        private readonly IJSONLoader _jsonLoader;

        private readonly IJSONSaver _jsonSaver;

        private readonly AcquisitionStarterImpl _acquisitionStarter;

        private TimeSpan _waitBeforeReadback;

        private TyphoonSystem? _typhoonSystem;

        private IHostApplicationLifetime? _hostApplicationLifetime;

        private bool _isOnline;

        private IHardwareControl? _HardwareControl;

        public RdaService(IHostApplicationLifetime hostApplicationLifetime,
            IOptions<SpectrumStreamSettings> config,
            ISysEnvironment sysEnvironment,
            IJSONLoader jsonLoader,
            IJSONSaver jsonSaver,
            AcquisitionStarterImpl acquisitionStarter,
            ILogger<RdaService> logger )
        {
            _acquisitionStarter = acquisitionStarter;

            _hostApplicationLifetime = hostApplicationLifetime;

            _waitForTyphoonOnlineTimeout = TimeSpan.FromSeconds(10);

            _config = config.Value;

            _logger = logger;

            _sysEnvironment = sysEnvironment;

            _jsonLoader = jsonLoader;

            _jsonSaver = jsonSaver;

            _acquisitionStarter = acquisitionStarter;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine( "RdaService is running...");

            CancellationTokenSource localCancelTokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                // By default, Ctrl + C would immediately kill the process.
                // If we set eventArgs.Cancel = true, we can perform a graceful shutdown first.
                eventArgs.Cancel = true;

                // Trigger the host to stop, letting it shut down gracefully
                //_lifetime.StopApplication();

                localCancelTokenSource.Cancel();
            };

            TimeSpan typhoonWaitTime = TimeSpan.FromMilliseconds( 500 );

            try
            {
                TyphoonSDK.Setup();

                var clientConfig = loadTyphoonConfig();

                _typhoonSystem = new TyphoonSystem( clientConfig);

                _typhoonSetupOk = true;

                if( _config.WaitForTyphoon )
                {
                    _logger.LogDebug( "Waiting for typhoon connection");

                    while( !_typhoonSystem.IsOnline && !stoppingToken.IsCancellationRequested) 
                    {                    
                        await Task.Delay( TimeSpan.FromSeconds( 1 ), stoppingToken );
                    }

                    _isOnline = _typhoonSystem.IsOnline;
                }
                else
                {
                    _logger.LogDebug( "Typhoon is supposed to be available");

                    TyphoonFactory.Instance.SystemManager.Start();
                }

                _HardwareControl = TyphoonFactory.Instance.HardwareControl;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize typhoon sdk");
            }

            try
            {
                if (_typhoonSetupOk)
                {
                    _acquisitionStarter.onTyphoonConnected( _typhoonSystem, _HardwareControl );

                    while (!stoppingToken.IsCancellationRequested && !localCancelTokenSource.IsCancellationRequested)
                    {
                        await Task.Delay(typhoonWaitTime, stoppingToken);

                        bool priorOnlineValue = _isOnline;

                        _isOnline = isOnline();

                        if( !priorOnlineValue && _isOnline )
                        {
                            logmessage( "Typhoon is becoming online...");
                        }
                        else
                        if( priorOnlineValue && !_isOnline )
                        {
                            logmessage( "Typhoon became unreachable...");
                        }
                    }
                }
                else
                {
                    while (!stoppingToken.IsCancellationRequested && !localCancelTokenSource.IsCancellationRequested)
                    {
                        await Task.Delay(typhoonWaitTime, stoppingToken);
                    }
                }
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Unrecoverable rda related feature" );
            }
            finally
            {                 
                try
                {
                    TyphoonFactory.Instance.SystemManager.Shutdown();
                }
                catch( Exception exShutdown )
                {
                    _logger?.LogError( exShutdown, "Typhoon shutdown failed" );
                }

                try
                {
                    _typhoonSystem?.Dispose();
                }
                catch( Exception exDispose )
                {
                    _logger.LogError( exDispose, "TyphoonSystem.Dispose failed" );
                }

            }

             _logger.LogDebug( "Rda service terminated");

        }

        public bool isOnline()
        {
            return _typhoonSystem?.IsOnline ?? false;
        }
        private TyphoonClientConfiguration loadTyphoonConfig()
        {
#if DEBUG
            var settingsFile = _sysEnvironment.fullPathFromExe("typhoonconfig.debug.json");
#else
            var settingsFile = _sysEnvironment.fullPathFromExe("typhoonconfig.json");
#endif
            var typhoonConfig = _sysEnvironment.fileExist(settingsFile) ? _jsonLoader.loadFromFile<TyphoonClientConfiguration>(settingsFile) : new TyphoonClientConfiguration()
            {
                StartupTyphoon = false,
                UseSimulatedInstrument = false
            };

            return typhoonConfig;
        }


        internal string logmessage( string message, Exception? ex = null ) {

            if ( ex is null )
            {
                _logger.LogDebug( message);
            }
            else
            {
                _logger.LogError( ex, message );
            }

            return message;

        }


    }
}

