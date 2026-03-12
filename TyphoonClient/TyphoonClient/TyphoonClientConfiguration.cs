//
// Copyright © 2014-2016 Waters Corporation. All Rights Reserved.
//


using System;

namespace Waters.Control.Client.Interface
{
    /// <summary>
    /// Configuration information for the typhoon client
    /// </summary>
    [Serializable]
    public class TyphoonClientConfiguration
    {
        /// <summary>
        /// Use the simulated instrument
        /// </summary>
        public bool UseSimulatedInstrument { get; set; }

        /// <summary>
        /// End point of the Typhoon directory service e.g. tcp://127.0.0.1:7777
        /// </summary>
        public string EndPointUri { get; set; } = "tcp://127.0.0.1:7777";

        /// <summary>
        /// The IP address of the instrument network that is passed to Typhoon
        /// </summary>
        public string InstrumentNetworkIPAddress { get; set; }

        /// <summary>
        /// True to attempt to start Typhoon local process, false to not start it
        /// </summary>
        public bool StartupTyphoon { get; set; } = true;

        /// <summary>
        /// Log folder to writer Typhoon log file in
        /// </summary>
        public string LogFolder { get; set; }

        /// <summary>
        /// Number of test connection retries to perform
        /// </summary>
        public int NumberOfTyphoonRetryTestConnections { get; set; } = 10;
    }
}
