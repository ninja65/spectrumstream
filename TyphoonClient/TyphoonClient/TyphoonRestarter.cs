////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2015-2016 Waters Corporation.
//
// Typhoon restarter implementation. Allows Typhoon to be restarted.
//
////////////////////////////////////////////////////////////////////////////

using System;
using Waters.Control.Client.Interface;

namespace Waters.Control.Client
{
    /// <summary>
    /// Typhoon restarter implementation. Allows Typhoon to be restarted.
    /// </summary>
    public class TyphoonRestarter : ITyphoonRestarter, IDisposable
    {
        private ISystemManager systemManager;
        private ISystemMonitor systemMonitor;
        private TyphoonClientConfiguration configuration;
        private bool restarting = false;
        private readonly bool systemManagerRestart = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="systemManager">system manager</param>
        /// <param name="systemMonitor">system monitor</param>
        /// <param name="configuration"></param>
        public TyphoonRestarter(ISystemManager systemManager, ISystemMonitor systemMonitor, TyphoonClientConfiguration configuration)
        {
            this.systemManager = systemManager;
            this.systemMonitor = systemMonitor;
            this.configuration = configuration;
            this.systemManagerRestart = systemManager.IsLegacyTyphoon() || configuration.UseSimulatedInstrument;
        }

        /// <summary>
        /// Restart the typhoon system.
        /// </summary>
        public void Restart()
        {
            if (systemManagerRestart)
            {
                if (systemMonitor.GetStatus() == SystemStatus.NoResponse)
                {
                    // If Typhoon is already shutdown and not running we just need to start it.
                    Console.WriteLine("Restarting typhoon");
                    systemManager.StartupTyphoon();
                }
                else
                {
                    systemMonitor.Status += OnSystemMonitorStatus;

                    // If Typhoon is either running or in the process of restarting then we need to shut it down first.
                    // When Typhoon does shutdown this will result in a call to OnSystemMonitorStatus with status=SystemStatus.NoResponse.
                    // When this occurs Typhoon can be started again.
                    restarting = true;
                    Console.WriteLine("Sending typhoon shutdown signal");
                    systemManager.Shutdown();
                }
            }
            else
            {
                // For typhoon onboard instrument just send the reboot operating system
                // the instrument OS will take care of restarting itself and Typhoon
                Console.WriteLine("Sending typhoon on instrument the soft reboot operating system signal");
                systemManager.SoftReboot();
            }
        }

        private void OnSystemMonitorStatus(SystemStatus status)
        {
            if (restarting && status == SystemStatus.NoResponse && systemManagerRestart)
            {
                restarting = false;
                systemMonitor.Status -= OnSystemMonitorStatus;

                Console.WriteLine("Restarting typhoon after shutdown");
                systemManager.StartupTyphoon();
            }
        }

        public void Dispose()
        {
            systemMonitor = null;
            systemManager = null;
        }
    }
}
