////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2013 Waters Corporation.
//
// Typhoon connection manager interface.
//
////////////////////////////////////////////////////////////////////////////

using Waters.Control.Message;

namespace Waters.Control.Client.InternalInterface
{
    /// <summary>
    /// Typhoon connection manager
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// Get the endpoint connection address
        /// </summary>
        /// <param name="service">The Typhoon service name</param>
        /// <param name="connectionType">The connection type</param>
        /// <param name="throwException">True if we want to throw an exception</param>
        /// <returns>The end point address</returns>
        string GetConnectAddress(string service,
           AddressQuery.Types.ConnectionType connectionType,
           bool throwException = true);

        /// <summary>
        /// Send and recive messages - returns the directory service address
        /// </summary>
        /// <param name="service">The Typhoon service name</param>
        /// <param name="connectionType">The connection type</param>
        /// <param name="throwException">True if we want to throw an exception</param>
        /// <returns>The end point address</returns>
        /// <exception cref="NoTyphoonResponseException" />
        /// <exception cref="ServiceNotFoundException" />
        string QueryDirectoryService(string service, AddressQuery.Types.ConnectionType connectionType, bool throwException = true);

        /// <summary>
        /// Test the connection to typhoon to see if it is up
        /// </summary>
        /// <param name="timeoutInMilliseconds">How long to wait for a connection to Typhoon</param>
        /// <returns>True if we can connect to Typhoon, False otherwise</returns>
        bool TestConnection(int timeoutInMilliseconds = 5000);
    }
}
