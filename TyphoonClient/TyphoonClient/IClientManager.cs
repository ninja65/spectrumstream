////////////////////////////////////////////////////////////////////////////
// 
// Copyright © 2018 Waters Corporation.
// 
// Interface to Client Manager Service functionality
// 
////////////////////////////////////////////////////////////////////////////

namespace Waters.Control.Client.Interface
{
    /// <summary>
    /// Interface to Client Manager Service functionality
    /// </summary>
    public interface IClientManager
    {
        /// <summary>
        /// Request the lock
        /// </summary>
        /// <param name="lockerTag">identifier of the requestor of the lock</param>
        /// <returns>True lock successful, false lock unsuccessful</returns>
        bool RequestLock(string lockerTag);

        /// <summary>
        /// Refresh the lock
        /// </summary>
        /// <param name="lockerTag">identifier of the refresher of the lock</param>
        /// <returns>True lock successful, false lock unsuccessful</returns>
        bool RefreshLock(string lockerTag);

        /// <summary>
        /// Release lock
        /// </summary>
        /// <param name="lockerTag">identifier of the requestor of the lock</param>
        /// <returns>True unlock successful, false unlock unsuccessful</returns>
        bool ReleaseLock(string lockerTag);
    }
}
