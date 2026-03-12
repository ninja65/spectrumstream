using System;

namespace Waters.Control.Client.Interface
{
    /// <summary>
    /// System manager interface
    /// </summary>
    public interface ISystemManager : IDisposable
    {
        /// <summary>
        /// Start the typhoon system
        /// </summary>
        void Start();

        /// <summary>
        /// Stop/shutdown the typhoon system
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Soft reboot the typhoon operating system (via typhoon)
        /// </summary>
        void SoftReboot();

        /// <summary>
        /// Notification about connection status changes between [UNIFI MS instrument driver/Typhoon-Client]-[Typhoon]
        /// </summary>
        event EventHandler<TyphoonConnectionEventArg> ConnectionStatusChanged;

        /// <summary>
        /// Set a function to call for log messages
        /// </summary>
        /// <param name="logFunc"></param>
        void SetLogger(Action<string> logFunc);

        /// <summary>
        /// Publish out the current connection status to any handlers
        /// </summary>
        void PublishConnectionStatus();

        /// <summary>
        /// Startup typhoon
        /// </summary>
        void StartupTyphoon();

        /// <summary>
        /// Check if legacy or full Typhoon
        /// </summary>
        bool IsLegacyTyphoon();
    }

}
