////////////////////////////////////////////////////////////////////////////
//
using NetMQ;
using System;
using System.Collections.Generic;
using NetMQ.Sockets;
using Waters.Control.Client.Interface;
using Waters.Control.Client.InternalInterface;
using Waters.Control.Message;
using System.Threading;

namespace Waters.Control.Client
{
    /// <summary>
    /// Connection manager to Typhoon system
    /// Responsable to sending message between the Typhoon-Client and Typhoon
    /// </summary>
    public class ConnectionManager : IConnectionManager
    {
        private readonly TyphoonClientConfiguration configuration;
        private bool connectionAvailable = false;

        public event Action Connected = () => { };

        public ConnectionManager(TyphoonClientConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Get the connection address
        /// </summary>
        /// <param name="service">The typhoon service name</param>
        /// <param name="connectionType">The socket connection type</param>
        /// <param name="throwException">True if we want to throw an exception</param>
        /// <returns>The endpoint address</returns>
        public string GetConnectAddress(string service, AddressQuery.Types.ConnectionType connectionType, bool throwException = true)
        {
            return QueryDirectoryService(service, connectionType, throwException);
        }

        /// <summary>
        /// Query directory service (Typhoon) - used for checking if the connection exists.
        /// </summary>
        /// <param name="service">The typhoon service name</param>
        /// <param name="connectionType">The socket connection type</param>
        /// <param name="throwException">True if we want to throw an exception</param>
        /// <returns>The endpoint address</returns>
        /// <exception cref="ServiceNotFoundException">Thrown if the specified service is not exists in the Typhoon.</exception></exception>
        public virtual string QueryDirectoryService(string service, AddressQuery.Types.ConnectionType connectionType, bool throwException = true)
        {
            var msgs = new List<string>(2);
            var addressQuery = new AddressQuery() { ConnectionType = connectionType, ServiceName = service };
            bool ok = false;
            int count = 100;

            // retry querying of the directory service if the result is not ok,
            // sometimes Typhoon is slow to start up and services are late registering
            // so try a few times before returning an error
            while (!ok && count > 0)
            {
                using (var request = new RequestSocket(configuration.EndPointUri))
                {
                    request.SendMoreFrame("Req.DirectoryService.AddressQuery");
                    request.SendFrame(MessageSerializer.Serialize(addressQuery));

                    ok = request.TryReceiveMultipartStrings(ConnectionTimeout, ref msgs);

                    if (!ok)
                    {
                        // the query failed, lets wait a while and then try again
                        Thread.Sleep(100);
                    }

                    count--;
                }
            }

            // if the query is still not ok throw an exception
            if (!ok)
            {
                throw new NoTyphoonResponseException();
            }

            string replyType = msgs[0];
            string addressReply = msgs[1];

            if (string.IsNullOrEmpty(replyType) || replyType == "Rep.DirectoryService.ServiceUnknown" || string.IsNullOrEmpty(addressReply))
            {
                if (throwException)
                    throw new ServiceNotFoundException(service);
                else
                    return replyType;
            }
            return UpdateAddressPath(service, addressReply);
        }

        public bool TestConnection(int timeoutInMilliseconds = 5000)
        {
            using (var request = new RequestSocket(configuration.EndPointUri))
            {
                request.Connect(configuration.EndPointUri);
                var addressQuery = new AddressQuery() { ServiceName = "ClientManager", ConnectionType = AddressQuery.Types.ConnectionType.Subscribe };
                request.SendMoreFrame("Req.DirectoryService.AddressQuery");
                request.SendFrame(MessageSerializer.Serialize(addressQuery));
                var msgs = new List<string>(2);

                connectionAvailable = request.TryReceiveMultipartStrings(TimeSpan.FromMilliseconds(timeoutInMilliseconds), ref msgs);

                return connectionAvailable;
            }
        }

        private TimeSpan ConnectionTimeout
        {
            get
            {
                int connectionTimeout = 10;

                var connectionTimeoutEnv = Environment.GetEnvironmentVariable("ClientConnectionTimeout");
                if (!string.IsNullOrWhiteSpace(connectionTimeoutEnv))
                {
                    connectionTimeout = int.Parse(connectionTimeoutEnv);
                }

                Console.WriteLine($"Client connecting to {configuration.EndPointUri} with timeout {connectionTimeout}s");
                return TimeSpan.FromSeconds(connectionTimeout);
            }
        }

        private string UpdateAddressPath(string service, string address)
        {
            // We get tcp://127.0.0.1:4567, we convert to tcp://<EndPoint>:4567
            var endpoint = configuration.EndPointUri.Replace(":7777", "");

            //Replace the simulator (127.0.0.1) or instrument address (192.168.0.1) with the address specified.
            string serviceAddress = address.Replace("tcp://127.0.0.1", endpoint).Replace("tcp://192.168.0.1", endpoint);

            return serviceAddress;
        }
    }
}
