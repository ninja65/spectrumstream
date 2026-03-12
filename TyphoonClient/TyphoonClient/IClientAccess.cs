////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2013 Waters Corporation.
//
// Typhoon client access interface.
//
////////////////////////////////////////////////////////////////////////////

using System;

namespace Waters.Control.Client.InternalInterface
{
    /// <summary>
    /// The typhoon client access interface
    /// </summary>
    public interface IClientAccess : IDisposable
    {
        /// <summary>
        /// Request  to the typhoon service
        /// </summary>
        /// <param name="service">The service name</param>
        /// <param name="messageId">The message id</param>
        void Request(string service, string messageId);

        /// <summary>
        /// Request to the typhoon service generic
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="service">The service name</param>
        /// <param name="messageId">The message id</param>
        /// <param name="messageObject">The message object</param>
        void Request<T>(string service, string messageId, T messageObject);


        /// <summary>
        /// Request to the typhoon service generic
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="service">The service name</param>
        /// <param name="messageId">The message id</param>
        /// <param name="messageObject">The message object</param>
        /// <param name="lockerTag">Locker tag holding client manager lock</param>
        void Request<T>(string service, string messageId, T messageObject, string lockerTag);

        /// <summary>
        /// Request reply to the typhoon service.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="messageId"></param>
        /// <returns>a byte array containing reply from service</returns>
        RequestReplyResult RequestReply<T>(string serviceName, string messageId, T requestMessage);

        /// <summary>
        /// Register a handler for a specific message id, provides message data as byte array.
        /// </summary>
        /// <param name="messageId">The message id</param>
        /// <param name="callback">Callback action</param>
        void RegisterHandler(string messageId, Action<byte[]> callback);
        /// <summary>
        /// Register a handler for a specific message id, ignore any message data.
        /// </summary>
        /// <param name="messageId">The message id</param>
        /// <param name="callback">Callback action</param>
        void RegisterHandler(string messageId, Action callback);
        /// <summary>
        /// Register a handler for a specific message id, deserialises message data to given protocol buffer.
        /// </summary>
        /// <typeparam name="T">Protocol buffer type</typeparam>
        /// <param name="messageId">The message id</param>
        /// <param name="callback">Callback action</param>
        void RegisterHandler<T>(string messageId, Action<T> callback) where T : new();
        /// <summary>
        /// Unregister handler for given message.
        /// </summary>
        /// <param name="messageId"></param>
        void UnregisterHandler(string messageId);
        /// <summary>
        /// Unregister handler for given message and action
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="callback"></param>
        void UnregisterHandler(string messageId, Action callback);
        /// <summary>
        /// Unregister handler for given message and action
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="callback"></param>
        void UnregisterHandler<T>(string messageId, Action<T> callback);
    }
}
