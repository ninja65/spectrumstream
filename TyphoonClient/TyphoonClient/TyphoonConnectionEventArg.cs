////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2014 Waters Corporation.
//
// Typhoon connection event argument
//
////////////////////////////////////////////////////////////////////////////

using System;

namespace Waters.Control.Client.Interface
{
    /// <summary>
    /// Typhoon connection event args. Holds connection status related data.
    /// </summary>
    public class TyphoonConnectionEventArg : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectAddress"></param>
        public TyphoonConnectionEventArg(TyphoonConnectionStatus connectStatus)
        {
            ConnectionStatus = connectStatus;
        }

        /// <summary>
        /// Get or set the connection status
        /// </summary>

        public TyphoonConnectionStatus ConnectionStatus { get; private set; }
    }

}
