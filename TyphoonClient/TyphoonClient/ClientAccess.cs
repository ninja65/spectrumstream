////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2013 Waters Corporation.
//
// Typhoon client access implementation.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using Waters.Control.Client.InternalInterface;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    /// <summary>
    /// Typhoon client access class.
    /// </summary>
    public class ClientAccess : IClientAccess, IDisposable
    {
        private IClientConnector connectionClient;

        /// <summary>
        /// dictionary message ids to list of handlers, each handler has a byte array handler action as a parameter (tuple item1), the byte array handler action may be a lander expression so the second
        /// part of the tuple contains the actual source handler action for use in deregistering the handler
        /// </summary>
        private readonly Dictionary<string, List<Tuple<Action<byte[]>, object>>> handlers = new Dictionary<string, List<Tuple<Action<byte[]>, object>>>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionClient">The typhoon client connector</param>
        public ClientAccess(IClientConnector connectionClient)
        {
            this.connectionClient = connectionClient;
            connectionClient.OnSubscribeEvent += HandleSubcriptionEvent;
        }

        public void Request(string service, string messageId)
        {
            Request(service, messageId, new byte[] { });
        }

        /// <summary>
        /// Request to the typhoon service generic
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="service">The service name</param>
        /// <param name="messageId">The message id</param>
        /// <param name="messageObject">The message object</param>
        public void Request<T>(string service, string messageId, T messageObject)
        {
            Request<T>(service, messageId, messageObject, null);
        }

        /// <summary>
        /// Request to the typhoon service generic
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="service">The service name</param>
        /// <param name="messageId">The message id</param>
        /// <param name="messageObject">The message object</param>
        /// <param name="lockerTag">Locker tag holding client manager lock</param>
        public void Request<T>(string service, string messageId, T messageObject, string lockerTag)
        {
            var clientRequest = new ClientRequest
            {
                Service = service,
                MessageId = messageId,
                MessageData = Google.Protobuf.ByteString.CopyFrom(MessageSerializer.Serialize(messageObject)),
                Tag = lockerTag ?? string.Empty
            };

            connectionClient.Push("ClientManager.ServiceRequest", MessageSerializer.Serialize(clientRequest));
        }

        public RequestReplyResult RequestReply<T>(string serviceName, string messageId, T requestMessage)
        {
            return connectionClient.RequestReply(serviceName, messageId, requestMessage);
        }

        /// <summary>
        /// Handle the socket subscription event from typhoon
        /// </summary>
        /// <param name="messageId">The message id</param>
        /// <param name="messageData">The message data</param>
        public void HandleSubcriptionEvent(string messageId, byte[] messageData)
        {
            lock (handlers)
            {
                if (handlers.ContainsKey(messageId))
                {
                    handlers[messageId].ForEach(o =>
                    {
                        try
                        {
                            o.Item1(messageData);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Typhoon message handler exception - message id:{messageId} with exception:{ex}");
                        }
                    });
                }
            }
        }

        private void AddHandler(string messageId, Tuple<Action<byte[]>, object> handler)
        {
            lock (handlers)
            {
                if (!handlers.ContainsKey(messageId))
                {
                    handlers[messageId] = new List<Tuple<Action<byte[]>, object>>();
                }

                handlers[messageId].Add(handler);
            }
        }

        public void RegisterHandler(string messageId, Action<byte[]> callback)
        {
            AddHandler(messageId, new Tuple<Action<byte[]>, object>(callback, callback));
        }

        public void RegisterHandler(string messageId, Action callback)
        {
            AddHandler(messageId, new Tuple<Action<byte[]>, object>(b => callback(), callback));
        }

        public void RegisterHandler<T>(string messageId, Action<T> callback) where T : new()
        {
            Action<byte[]> deserialiseMessageAction = b =>
            {
                try
                {
                    var deserialisedPayload = MessageSerializer.Deserialize<T>(b);

                    if (deserialisedPayload == null)
                    {
                        Console.WriteLine($"Error deserialised message payload is null for message {messageId}");
                    }

                    callback(deserialisedPayload);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Typhoon message failed to deserialise - message id:{messageId} message type:{typeof(T).Name} with exception:{ex}");
                }
            };

            AddHandler(messageId, new Tuple<Action<byte[]>, object>(deserialiseMessageAction, callback));
        }


        private void RemoveHandler(string messageId, object handler)
        {
            lock (handlers)
            {
                if (handlers.ContainsKey(messageId))
                {
                    var findHandler = handlers[messageId].Where(o => o.Item2 == handler);
                    if (findHandler.Any())
                    {
                        handlers[messageId].Remove(findHandler.First());
                    }
                }
            }
        }

        /// <summary>
        /// Unregister ALL handler of the message type
        /// </summary>
        /// <param name="messageId"></param>
        public void UnregisterHandler(string messageId)
        {
            lock (handlers)
            {
                if (handlers.ContainsKey(messageId))
                {
                    handlers.Remove(messageId);
                }
            }
        }

        public void UnregisterHandler(string messageId, Action callback)
        {
            RemoveHandler(messageId, (object)callback);
        }

        /// <summary>
        /// Unregister handler for given message and action
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="callback"></param>
        public void UnregisterHandler<T>(string messageId, Action<T> callback)
        {
            RemoveHandler(messageId, (object)callback);
        }

        /// <summary>
        /// Dispose this class
        /// </summary>
        public void Dispose()
        {
            lock (handlers)
            {
                handlers.Clear();
            }
            connectionClient.OnSubscribeEvent -= HandleSubcriptionEvent;
            connectionClient = null;
        }
    }
}
