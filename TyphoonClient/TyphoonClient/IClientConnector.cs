////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2013 Waters Corporation.
//
// Typhoon client connector interface.
//
////////////////////////////////////////////////////////////////////////////

using System;

namespace Waters.Control.Client.InternalInterface
{
    /// <summary>
    /// The client connector interface
    /// </summary>
    public interface IClientConnector : IDisposable
    {
        /// <summary>
        /// On subscription event
        /// </summary>
        event Action<string, byte[]> OnSubscribeEvent;

        bool IsConnected { get; }

        void SetupConnection();

        /// <summary>
        /// Push the message with data
        /// </summary>
        /// <param name="messagingId">The message id</param>
        /// <param name="messageData">The message data</param>
        void Push(string messagingId, byte[] messageData);

        RequestReplyResult RequestReply<T>(string serviceName, string messageId, T requestMessage);

        /// <summary>
        /// Reset sockets
        /// </summary>
        void Reset();

        /// <summary>
        /// Find out if a certain Typhoon service is running
        /// </summary>
        bool IsServiceRunning(string serviceName);
    }
}
