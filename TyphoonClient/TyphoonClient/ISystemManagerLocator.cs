//
// Copyright © 2014 Waters Corporation. All Rights Reserved.
//


namespace Waters.Control.Client.InternalInterface
{
    /// <summary>
    /// Interface to find Typhoon system manager.
    /// </summary>
    public interface ISystemManagerLocator
    {
        /// <summary>
        /// Get the fully qualified filename of the Typhoon system manager.
        /// </summary>
        /// <returns></returns>
        string GetSystemManager();
    }

}
