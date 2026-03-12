////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2015-2016 Waters Corporation.
//
// Typhoon restarter interface. Allows Typhoon to be restarted.
//
////////////////////////////////////////////////////////////////////////////

namespace Waters.Control.Client.Interface
{
    /// <summary>
    /// Typhoon restarter interface. Allows Typhoon to be restarted.
    /// </summary>
    public interface ITyphoonRestarter
    {
        /// <summary>
        /// Restart Typhoon.
        /// </summary>
        void Restart();
    }
}
