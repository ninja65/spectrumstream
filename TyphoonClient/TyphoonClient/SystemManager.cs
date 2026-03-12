using System;
using System.Threading.Tasks;
using Waters.Control.Client.Interface;
using Waters.Control.Client.InternalInterface;

namespace Waters.Control.Client
{
    /// <summary>
    /// Typhoon system manager implementation
    /// </summary>
    public class SystemManager : ISystemManager
    {

        /// <summary>
        /// Notification about the changes in the connection status between Typhoon - TyphoonClient(UNIFI MS instrument driver)
        /// </summary>
        public event EventHandler<TyphoonConnectionEventArg> ConnectionStatusChanged = (o, a) => { };

        private readonly TyphoonClientConfiguration configuration;
        private readonly ITyphoonStarter typhoonStarter;
        private readonly Lazy<IClientAccess> clientAccess;
        private readonly Lazy<IKeyValueStore> keyValueStore;
        private readonly Lazy<ISystemMonitor> systemMonitor;
        private readonly Lazy<IClientConnector> clientConnector;
        private readonly Lazy<IConnectionTester> connectionTester;
        private Task typhoonConnectionTestingTask;
        private TyphoonConnectionStatus currentConnectionStatus;
        private bool connectionInitialised;
        private bool typhoonConnectionTestingActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemManager"/> class.
        /// </summary>
        /// <param name="startTyphoon"></param>
        /// <param name="clientAccess"></param>
        /// <param name="keyValueStore"></param>
        /// <param name="systemMonitor"></param>
        /// <param name="connectionTester"></param>
        /// <param name="configuration"></param>
        public SystemManager(
            TyphoonClientConfiguration configuration,
            IConnectionManager connManager,
            ITyphoonStarter startTyphoon,
            Lazy<IClientConnector> clientConnector,
            Lazy<IClientAccess> clientAccess,
            Lazy<ISystemMonitor> systemMonitor,
            Lazy<IConnectionTester> connectionTester,
            Lazy<IKeyValueStore> keyValueStore)
        {
            this.configuration = configuration;
            typhoonStarter = startTyphoon;
            this.clientConnector = clientConnector;
            this.clientAccess = clientAccess;
            this.systemMonitor = systemMonitor;
            this.connectionTester = connectionTester;
            this.keyValueStore = keyValueStore;

            currentConnectionStatus = TyphoonConnectionStatus.UNKNOWN;
        }

        /// <summary>
        /// Start the typhoon system
        /// First we check the connection. If the connection fails try to start Typhoon and check the connection again
        /// </summary>
        public void Start()
        {
            if (configuration.StartupTyphoon)
            {
                StartupTyphoon();
            }

            // try to connect to Typhoon
            var connectionWaitTask = connectionTester.Value.WaitForConnection(configuration.NumberOfTyphoonRetryTestConnections);
            connectionWaitTask.Wait();

            //the number of retries to connect have been exceeded, send failed event and start testing for
            //the connection until it succeeds
            if (connectionWaitTask.Result != TyphoonConnectionStatus.SUCCEEDED)
            {
                OnConnectionStatusChanged(TyphoonConnectionStatus.FAILED);

                // asynchronous call control will return here immediately
                StartTyphoonConnectionTesting();
            }
            else // initial connection test has succeeded
            {
                // now the connection to Typhoon has been tested, now setup a proper
                // connection to Typhoon
                PerformConnectedActions();
                OnConnectionStatusChanged(TyphoonConnectionStatus.SUCCEEDED);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void StartupTyphoon()
        {
            typhoonStarter.Start();
        }

        /// <summary>
        /// Perform actions necessary for a connection to Typhoon
        /// </summary>
        private void PerformConnectedActions()
        {
            clientConnector.Value.SetupConnection();
            keyValueStore.Value.Reset();
            systemMonitor.Value.Status += OnSystemMonitorStatusChanged;
            systemMonitor.Value.StartMonitoringConnection();
            connectionInitialised = true;
        }


        /// <summary>
        /// Called for connecting to Typhoon in situations: after initial failed connection attempt, testing for typhoon connection after heartbeat missed or a typhoon restart
        /// </summary>
        private void StartTyphoonConnectionTesting()
        {
            if (typhoonConnectionTestingTask == null || typhoonConnectionTestingTask.IsCompleted)
            {
                typhoonConnectionTestingActive = true;
                typhoonConnectionTestingTask = Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            // use the connection tester to send the request
                            var connectionStatus = await connectionTester.Value.WaitForConnection(-1);
                            if (connectionStatus == TyphoonConnectionStatus.SUCCEEDED)
                            {
                                // reset the push and subscribe sockets
                                clientConnector.Value.Reset();

                                if (!connectionInitialised) PerformConnectedActions();

                                // have to guarantee that Typhoon up and stable, so wait for the typhoon heartbeat to be heard
                                if (systemMonitor.Value.WaitForHeartbeat())
                                {
                                    OnConnectionStatusChanged(TyphoonConnectionStatus.SUCCEEDED);
                                    StartResyncOfRooms();
                                    break;
                                }
                                else
                                {
                                    OnConnectionStatusChanged(TyphoonConnectionStatus.FAILED);
                                }
                            }
                            else
                            {
                                OnConnectionStatusChanged(TyphoonConnectionStatus.FAILED);
                            }
                        }
                    }
                    finally
                    {
                        typhoonConnectionTestingActive = false;
                    }
                });
            }
            else
            {
                Console.WriteLine("SystemManager Connection to typhoon is already being tested");
            }
        }


        private void StartResyncOfRooms()
        {
            Task.Run(() =>
            {
                try
                {
                    Console.WriteLine("SystemManager Typhoon has restarted, initiating reset of key-value store");
                    keyValueStore.Value.Reset();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SystemManager Error initiating reset of key-value store due to Typhoon restart - {ex}");
                }
            });
        }


        /// <summary>
        /// Handle the SystemMonitor SystemStatus changed event.
        /// Typhoon heartbeat system
        /// </summary>
        /// <param name="systemStatus">The system status.</param>
        private void OnSystemMonitorStatusChanged(SystemStatus systemStatus)
        {
            // Typhoon is disconnected or restarted
            if (!typhoonConnectionTestingActive && (systemStatus == SystemStatus.NoResponse || systemStatus == SystemStatus.Restarted))
            {
                Console.WriteLine($"SystemManager Typhoon connection status {systemStatus}, starting test for connection");
                OnConnectionStatusChanged(TyphoonConnectionStatus.FAILED);
                StartTyphoonConnectionTesting();
            }
        }

        /// <summary>
        /// Stop/shutdown the typhoon system
        /// </summary>
        public void Shutdown()
        {
            clientAccess.Value.Request("SystemManager", "System.Shutdown", new byte[0]);
        }

        /// <summary>
        /// Soft reboot the typhoon operating system (via typhoon)
        /// </summary>
        public void SoftReboot()
        {
            if (IsLegacyTyphoon())
            {
                //legacy Typhoon, perform reboot via message to LegacyConnector
                Console.WriteLine($"SystemManager SoftReboot: LegacyService.RebootEPC sent to LegacyConnector");
                clientAccess.Value.Request("LegacyConnector", "LegacyService.RebootEPC");
            }
            else
            {
                //full Typhoon, perform reboot via message to SoftwareUpdater
                Console.WriteLine($"SystemManager SoftReboot: SoftwareUpdater.SoftReboot sent to SoftwareUpdater");
                clientAccess.Value.Request("SoftwareUpdater", "SoftwareUpdater.SoftReboot");
            }
        }

        /// <summary>
        /// Fire the ConnectionStatusChanged event
        /// </summary>
        internal void OnConnectionStatusChanged(TyphoonConnectionStatus newConnectionStatus)
        {
            currentConnectionStatus = newConnectionStatus;
            ConnectionStatusChanged(this, new TyphoonConnectionEventArg(newConnectionStatus));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="logFunc"></param>
        public void SetLogger(Action<string> logFunc)
        {
        }


        /// <summary>
        /// Publish out the current connection status to any handlers
        /// </summary>
        public void PublishConnectionStatus()
        {
            Task.Run(() =>
            {
                try
                {
                    OnConnectionStatusChanged(currentConnectionStatus);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SystemManager Error publishing connection status {ex}");
                }
            });
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            if (connectionInitialised) systemMonitor.Value.Status -= OnSystemMonitorStatusChanged;
        }

        /// <summary>
        /// Check if legacy or full Typhoon
        /// </summary>
        public bool IsLegacyTyphoon()
        {
            return clientConnector.Value.IsServiceRunning("LegacyConnector");
        }
    }
}
