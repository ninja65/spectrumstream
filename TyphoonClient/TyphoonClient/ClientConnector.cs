////////////////////////////////////////////////////////////////////////////
//
// Copyright � 2013 Waters Corporation.
//
// Typhoon client connector implementation.
//
////////////////////////////////////////////////////////////////////////////

using NetMQ;
using NetMQ.Sockets;
using System;
using Waters.Control.Client.InternalInterface;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    /// <summary>
    /// The client connector class
    /// </summary>
    public class ClientConnector : IClientConnector, IDisposable
    {
        private SubscriberSocket subscriberSocket = new SubscriberSocket();
        private PushSocket pushSocket = new PushSocket();
        private readonly IConnectionManager connectionManager;
        private NetMQPoller poller = new NetMQPoller();
        private bool isConnected = false;

        /// <summary>
        /// On subscription event
        /// </summary>
        public event Action<string, byte[]> OnSubscribeEvent;
        /// <summary>
        ///
        /// </summary>
        /// <param name="connectionManager"></param>
        public ClientConnector(IConnectionManager connectionManager)
        {
            SetShutdownLingerTime();
            this.connectionManager = connectionManager;
        }

        /// <summary>
        /// Has the connection been setup
        /// </summary>
        public bool IsConnected => isConnected;

        /// <summary>
        /// Setup the connection by creating subscriber and push socket and poller to manage incoming subscriber messages
        /// Typhoon is expected to be running and connectable before this is called
        /// </summary>
        public void SetupConnection()
        {
            string address = connectionManager.GetConnectAddress("ClientManager", AddressQuery.Types.ConnectionType.Subscribe);

            subscriberSocket.Connect(address);

            subscriberSocket.SubscribeToAnyTopic();

            subscriberSocket.ReceiveReady += HandleIncommingMessage;

            poller.Add(subscriberSocket);
            poller.RunAsync();

            string connectAddress = connectionManager.GetConnectAddress("ClientManager", AddressQuery.Types.ConnectionType.Connect);

            pushSocket.Connect(connectAddress);
            
            
            isConnected = true;
        }

        /// <summary>
        /// Push the message with data
        /// </summary>
        /// <param name="messagingId">The message id</param>
        /// <param name="messageData">The message data</param>
        public void Push(string messagingId, byte[] messageData)
        {
            pushSocket.SendMoreFrame(messagingId);
            pushSocket.SendFrame(messageData);
        }
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceName"></param>
        /// <param name="messageId"></param>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public RequestReplyResult RequestReply<T>(string serviceName, string messageId, T requestMessage)
        {
            if (!isConnected) throw new ConnectionUnavailableException();
            var result = new RequestReplyResult();

            using (var request = new RequestSocket(connectionManager.GetConnectAddress(serviceName, AddressQuery.Types.ConnectionType.Request)))
            {
                request.SendMoreFrame(messageId);
                request.SendFrame(MessageSerializer.Serialize(requestMessage));

                result.ReplyId = request.ReceiveFrameString();
                result.MessageData = request.ReceiveFrameBytes();
            }

            return result;
        }

        private void HandleIncommingMessage(object sender, NetMQSocketEventArgs e)
        {
            string messageId = e.Socket.ReceiveFrameString();
            byte[] messageData = e.Socket.ReceiveFrameBytes();

            if (OnSubscribeEvent != null)
            {
                OnSubscribeEvent.Invoke(messageId, messageData);
            }
        }

        /// <summary>
        /// Reset subscriber and push sockets
        /// </summary>
        public void Reset()
        {
            if (isConnected)
            {
                var addrSubscriber = connectionManager.GetConnectAddress("ClientManager", AddressQuery.Types.ConnectionType.Subscribe);
                var addrPush = connectionManager.GetConnectAddress("ClientManager", AddressQuery.Types.ConnectionType.Connect);

                poller.Remove(subscriberSocket);
                poller.Remove(pushSocket);

                subscriberSocket.Disconnect(addrSubscriber);
                subscriberSocket.Connect(addrSubscriber);

                pushSocket.Disconnect(addrPush);
                pushSocket.Connect(addrPush);

                poller.Add(subscriberSocket);
                poller.Add(pushSocket);
            }
        }

        /// <summary>
        /// Dispose this instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void SetShutdownLingerTime()
        {
            // Set the maximum time to wait for pending outgoing messages to be sent when shutting down, if the message
            // cannot be sent within this time the message is dropped.
            NetMQConfig.Linger = TimeSpan.FromMilliseconds(100);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (isConnected)
                {
                    if (poller != null)
                    {
                        poller.StopAsync();
                        poller.Dispose();
                        poller = null;
                    }

                    if (pushSocket != null)
                    {
                        pushSocket.Dispose();
                        pushSocket = null;
                    }

                    if (subscriberSocket != null)
                    {
                        subscriberSocket.ReceiveReady -= HandleIncommingMessage;
                        subscriberSocket.Dispose();
                        subscriberSocket = null;
                    }
                }
                NetMQConfig.Cleanup(false);
            }
        }

        /// <summary>
        /// Find out if a certain typhoon service exists and is running
        /// </summary>
        public bool IsServiceRunning(string serviceName)
        {
            string response = connectionManager.GetConnectAddress(serviceName, AddressQuery.Types.ConnectionType.Subscribe, false);
            return response != "Rep.DirectoryService.ServiceUnknown";
        }
    }
}
