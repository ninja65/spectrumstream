////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2015 Waters Corporation.
//
// System monitor interface.
//
////////////////////////////////////////////////////////////////////////////

using System;

namespace Waters.Control.Client.Interface
{
    /// <summary>
    /// Typhoon system status.
    /// </summary>
    public enum SystemStatus { OK, NoResponse, Restarted };

    /// <summary>
    /// System monitor interface.
    /// </summary>
    public interface ISystemMonitor : IDisposable
    {
        /// <summary>
        /// Get the Typhoon system status.
        /// </summary>
        /// <returns>Typhoon system status.</returns>
        SystemStatus GetStatus();

        /// <summary>
        /// Called when Typhoon system status changes.
        /// </summary>
        event Action<SystemStatus> Status;

        /// <summary>
        /// Typhoon Timeout Interval in seconds
        /// </summary>
        double TimeoutInterval {set;}

        /// <summary>
        /// Start monitoring the connection to Typhoon
        /// </summary>
        void StartMonitoringConnection();

        /// <summary>
        /// Publish system status to Status observers
        /// </summary>
        void PublishSystemStatus();

        /// <summary>
        /// WAit for next good heartbeat
        /// </summary>
        /// <returns></returns>
        bool WaitForHeartbeat();
    }
}
