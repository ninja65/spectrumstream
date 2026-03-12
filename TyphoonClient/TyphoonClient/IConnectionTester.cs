////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2018 Waters Corporation.
//
////////////////////////////////////////////////////////////////////////////

using System.Threading.Tasks;
using Waters.Control.Client.Interface;

namespace Waters.Control.Client.InternalInterface
{
    /// <summary>
    /// Utility interface to test for connection to Typhoon
    /// </summary>
    public interface IConnectionTester
    {
        /// <summary>
        /// Wait for connection with infinite number of retries
        /// </summary>
        /// <returns></returns>
        Task<TyphoonConnectionStatus> WaitForConnection();

        /// <summary>
        /// Wait for connection trying the specified number of times
        /// </summary>
        /// <param name="numberOfRetryTestConnections"></param>
        /// <returns></returns>
        Task<TyphoonConnectionStatus> WaitForConnection(int numberOfRetryTestConnections);
    }
}
