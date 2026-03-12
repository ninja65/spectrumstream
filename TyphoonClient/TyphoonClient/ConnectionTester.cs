////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2013-2018 Waters Corporation.
//
// Utility class for detecting when connection with Typhoon is reestablished
//
////////////////////////////////////////////////////////////////////////////

using Serilog;
using Serilog.Events;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Waters.Control.Client.Interface;
using Waters.Control.Client.InternalInterface;

namespace Waters.Control.Client
{
    /// <summary>
    /// Utility class to test for connection to Typhoon
    /// </summary>
    public class ConnectionTester : IConnectionTester, IDisposable
    {
        private IConnectionManager connectionManager;
        private Task<TyphoonConnectionStatus> connectingTask;
        private readonly object lockObject = new object();
        private bool cancel;

        /// <summary>
        /// Delay between attempts to send Typhoon a message
        /// </summary>
        public int DelayBetweenTestConnectionAttemptsMs { get; set; }

        public ConnectionTester(IConnectionManager connectionManager, int delayBetweenTestConnectionAttemptsMs = 500)
        {
            this.connectionManager = connectionManager;
            DelayBetweenTestConnectionAttemptsMs = delayBetweenTestConnectionAttemptsMs;
        }

        /// <summary>
        /// Wait for connection with infinite number of retries
        /// </summary>
        /// <returns></returns>
        public async Task<TyphoonConnectionStatus> WaitForConnection()
        {
            return await WaitForConnection(-1);
        }
        /// <summary>
        /// Wait for connection trying the specified number of times
        /// </summary>
        /// <param name="numberOfRetryTestConnections"></param>
        /// <returns></returns>
        public async Task<TyphoonConnectionStatus> WaitForConnection(int numberOfRetryTestConnections)
        {
            Log.Logger.Write(LogEventLevel.Debug, "WaitForConnection");

            TyphoonConnectionStatus status;
            Task<TyphoonConnectionStatus> waitTask;
            lock (lockObject)
            {
                if (connectingTask == null)
                {
                    // create the task that will be performing the connection attempts
                    connectingTask = TestTyphoonConnection(numberOfRetryTestConnections);
                }
                waitTask = connectingTask;
            }

            try
            {
                // wait for the connection to be made or the attempt to run out
                status = await waitTask;
            }
            finally
            {
                lock (lockObject)
                {
                    // the connecting task is complete so nullify it for next time
                    connectingTask = null;
                }
            }

            return status;
        }


        private async Task<TyphoonConnectionStatus> TestTyphoonConnection(int numberOfRetryTestConnections)
        {
            int connectionTries = numberOfRetryTestConnections;
            TyphoonConnectionStatus status = TyphoonConnectionStatus.UNKNOWN;

            // if numberOfRetryTestConnections is -1 then this will loop permanently until a connection is made
            do
            {
                status = await TestTyphoonConnection();

                if (connectionTries > 0)
                {
                    connectionTries--;
                }

                if (connectionTries > 0)
                {
                    // delay a bit before the next attempt
                    await Task.Delay(DelayBetweenTestConnectionAttemptsMs);
                }
            }
            while (connectionTries != 0 && status != TyphoonConnectionStatus.SUCCEEDED && !cancel);


            return status;
        }

        /// <summary>
        /// Test the connection to the Typhoon. Sends a message to the Typhoon.
        /// </summary>
        private async Task<TyphoonConnectionStatus> TestTyphoonConnection()
        {
            return await Task.Run(() =>
            {
                try
                {
                    return connectionManager.TestConnection(500) ? TyphoonConnectionStatus.SUCCEEDED : TyphoonConnectionStatus.FAILED;
                }
                catch (NullReferenceException ex)
                {
                    Log.Logger.Write(LogEventLevel.Debug, "TestTyphoonConnection - NullReferenceobject" );
                    if (ex.InnerException != null)
                    {
                        Log.Logger.Write(LogEventLevel.Debug, ex.InnerException.StackTrace);
                    }
                    Log.Logger.Write(LogEventLevel.Debug, ex.Message);
                    return TyphoonConnectionStatus.FAILED;
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Log.Logger.Write(LogEventLevel.Debug, ex.InnerException.StackTrace);
                    }
                    Log.Logger.Write(LogEventLevel.Debug, ex.Message);
                    return TyphoonConnectionStatus.FAILED;
                }
            });
        }


        public void Dispose()
        {
            connectionManager = null;
            cancel = true;
        }
    }
}
