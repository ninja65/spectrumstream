//*******************************************************************************************************************
//	Copyright (c) © 2015 Waters Corporation. All rights reserved.
//
//*********************************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using Waters.Control.Client.InternalInterface;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    public interface IKeyValueRoom : IEnumerable<KeyValuePair<string, byte[]>>
    {
        event Action<string> KeyChanged;
        event Action<KeyValue> KeyValueChanged;

        string Name { get; }
        void Put<T>(string key, T value) where T : class;
        T Get<T>(string key) where T : class, new();
        T Get<T>(string key, T defaultValue) where T : class, new();

        /// <summary>
        /// Make a copy of the current cache's keys, this will a copy hence will not throw should the cache's set of key change
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetKeys();

        /// <summary>
        /// Make a copy of the current cache's keys, then project the elements using the selector Function.
        /// </summary>
        /// <param name="selector">The select condition.</param>
        /// <returns></returns>
        IEnumerable<T> GetKeys<T>(Func<string, T> selector);

        bool Exists(string key);
        void Delete(string key);
        void Clear(bool close = false);
        void Subscribe<T>(string key, Action<T> callback) where T : class, new();
        void UnsubscribeKey(string key);
        void SubscribeDelete(string key, Action callback);
        void UpdateFlags(KeyValueRoom.Flags flags);
        void OnUpdate(KeyValueUpdate update);

        /// <summary>
        /// Send the named key-value contained in the room out to any listening subscribers
        /// </summary>
        /// <param name="key"></param>
        void SendNamedKeyValueToSubscribers(string key);

        /// <summary>
        /// Send all key-value contained in the room out to any listening subscribers
        /// </summary>
        void SendAllKeyValuesToSubscribers();
    }

    public class KeyValueRoom : IKeyValueRoom
    {
        public enum Flags { InMemory, Persisted };

        private readonly Dictionary<string, byte[]> cachedValues = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, List<Action<byte[]>>> subscribers = new Dictionary<string, List<Action<byte[]>>>();
        private readonly Dictionary<string, List<Action>> deleteSubscribers = new Dictionary<string, List<Action>>();
        private readonly string roomName;
        private readonly IKeyValueStore store;
        private Flags flags;
        private ulong sequenceId = 0;
        private readonly ReaderWriterLock readWriteCacheLock = new ReaderWriterLock();
        private readonly object subscribersLock = new object();
        private const int LockTimeout = 5000;

        public event Action<string> KeyChanged = e => { };
        public event Action<KeyValue> KeyValueChanged = e => { };

        public string Name { get { return roomName; } }

        internal KeyValueRoom(IKeyValueStore store, string roomName, Flags flags)
        {
            this.store = store;
            this.roomName = roomName;
            this.flags = flags;
        }

        public void Put<T>(string key, T value) where T : class
        {
            var data = MessageSerializer.Serialize(value);

            readWriteCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                cachedValues[key] = data;

            }
            finally
            {
                readWriteCacheLock.ReleaseWriterLock();
            }

            // send the update to the KVS outside the write lock
            // to allow readers to continue as this may take some time
            var putUpdate = new KeyValueStorePut { Room = roomName };
            putUpdate.Items.Add(new KeyValue { Key = key, Value = ByteString.CopyFrom(data) });
            store.SyncPut(putUpdate);
        }

        public T Get<T>(string key) where T : class, new()
        {
            byte[] value;
            readWriteCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                if (!cachedValues.ContainsKey(key))
                {
                    throw new KeyNotFoundException($"Failed to find key '{key}' in key value room '{roomName}'");
                }

                value = cachedValues[key];
            }
            finally
            {
                readWriteCacheLock.ReleaseReaderLock();
            }
            return MessageSerializer.Deserialize<T>(value);

        }

        public T Get<T>(string key, T defaultValue) where T : class, new()
        {
            readWriteCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                return Exists(key) ? Get<T>(key) : defaultValue;
            }
            finally
            {
                readWriteCacheLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Make a copy of the current cache's keys, this will a copy hence will not throw should the cache's set of key change
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetKeys()
        {
            readWriteCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                return cachedValues.Select(o => o.Key).ToList();
            }
            finally
            {
                readWriteCacheLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Make a copy of the current cache's keys, then project the elements using the selector Function.
        /// </summary>
        /// <param name="selector">The select condition.</param>
        /// <returns></returns>
        public IEnumerable<T> GetKeys<T>(Func<string, T> selector)
        {
            readWriteCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                //ToList() to avoid race condition during deferred execution
                return cachedValues.Keys.Select(selector).ToList();
            }
            finally
            {
                readWriteCacheLock.ReleaseReaderLock();
            }
        }

        public bool Exists(string key)
        {
            readWriteCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                return cachedValues.ContainsKey(key);
            }
            finally
            {
                readWriteCacheLock.ReleaseReaderLock();
            }
        }

        public void Delete(string key)
        {
            readWriteCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                if (cachedValues.ContainsKey(key))
                {
                    cachedValues.Remove(key);
                }
            }
            finally
            {
                readWriteCacheLock.ReleaseWriterLock();
            }

            // send the update to the KVS outside the write lock
            // to allow readers to continue as this may take some time
            var deleteUpdate = new KeyValueStorePut { Room = roomName };
            deleteUpdate.Items.Add(new KeyValue { Key = key });
            store.SyncPut(deleteUpdate);
        }

        public void Clear(bool close = false)
        {
            var clearUpdate = new KeyValueStoreClear
            {
                Room = roomName,
                Close = close
            };
            readWriteCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                cachedValues.Clear();
            }
            finally
            {
                readWriteCacheLock.ReleaseWriterLock();
            }

            // send the update to the KVS outside the write lock
            // to allow readers to continue as this may take some time
            store.SyncClear(clearUpdate);
        }

        public void Subscribe<T>(string key, Action<T> callback) where T : class, new()
        {
            if (Monitor.TryEnter(subscribersLock, LockTimeout))
            {
                try
                {
                    if (!subscribers.ContainsKey(key))
                    {
                        subscribers[key] = new List<Action<byte[]>>();
                    }

                    subscribers[key].Add(b => callback(MessageSerializer.Deserialize<T>(b)));

                    if (Exists(key))
                    {
                        callback(Get<T>(key));
                    }
                }
                finally
                {
                    Monitor.Exit(subscribersLock);
                }
            }
            else
            {
                throw new TimeoutException("Timeout acquiring lock on list of subscribers in room: " + roomName);
            }
        }

        public void UnsubscribeKey(string key)
        {
            if (Monitor.TryEnter(subscribersLock, LockTimeout))
            {
                try
                {
                    if (subscribers.ContainsKey(key))
                    {
                        subscribers.Remove(key);
                    }
                }
                finally
                {
                    Monitor.Exit(subscribersLock);
                }
            }
            else
            {
                throw new TimeoutException("Timeout acquiring lock on list of subscribers in room: " + roomName);
            }
        }

        public void SubscribeDelete(string key, Action callback)
        {
            if (Monitor.TryEnter(subscribersLock, LockTimeout))
            {
                try
                {
                    if (!deleteSubscribers.ContainsKey(key))
                    {
                        deleteSubscribers[key] = new List<Action>();
                    }

                    deleteSubscribers[key].Add(callback);

                    if (!Exists(key))
                    {
                        callback();
                    }
                }
                finally
                {
                    Monitor.Exit(subscribersLock);
                }
            }
            else
            {
                throw new TimeoutException("Timeout acquiring lock on list of delete subscribers in room:" + roomName);
            }
        }

        public void UpdateFlags(Flags flagsUpdate)
        {
            if (flags != flagsUpdate && flags != Flags.Persisted)
            {
                flags = flagsUpdate;
                Resync();
            }
        }

        public void OnUpdate(KeyValueUpdate update)
        {
            if (update.SequenceId == sequenceId + 1)
            {
                UpdateCache(update);
            }
            else if (update.SequenceId > sequenceId + 1)
            {
                Resync();
            }
        }


        public void SendNamedKeyValueToSubscribers(string key)
        {
            readWriteCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                SendKeyValuesToSubscribers(cachedValues.Where(cv => cv.Key.Contains(key)).Select(kvp => new KeyValue() { Key = kvp.Key, Value = ByteString.CopyFrom(kvp.Value) }));
            }
            finally
            {
                readWriteCacheLock.ReleaseReaderLock();
            }
        }

        public void SendAllKeyValuesToSubscribers()
        {
            readWriteCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                SendKeyValuesToSubscribers(cachedValues.Select(kvp => new KeyValue() { Key = kvp.Key, Value = ByteString.CopyFrom(kvp.Value) }));
            }
            finally
            {
                readWriteCacheLock.ReleaseReaderLock();
            }
        }

        private void SendKeyValuesToSubscribers(IEnumerable<KeyValue> kvs)
        {
            foreach (var kv in kvs)
            {
                SendKeyChangedEvent(kv);
                if (kv.Value != null)
                {
                    SendKeyChangedEventToSubscribers(kv);
                }
                else
                {
                    SendKeyDeletedEventToDeleteSubscribers(kv);
                }
            }
        }

        internal void Resync()
        {
            // room will be resync in a writers lock, this prevent readers from accessing
            // until the resync is complete
            readWriteCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                cachedValues.Clear();

                // this may take a while, all readers will locked out until done
                var update = store.SyncRoom(roomName, flags);

                UpdateCache(update);
            }
            finally
            {
                readWriteCacheLock.ReleaseWriterLock();
            }

        }

        private void UpdateCache(KeyValueUpdate update)
        {
            readWriteCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                sequenceId = update.SequenceId;

                // update the cache first whilst in the write lock
                foreach (var kv in update.Item)
                {
                    if (kv.Value != null)
                    {
                        cachedValues[kv.Key] = kv.Value.ToByteArray();
                    }
                    else
                    {
                        // Update with key with value not set means delete the entry.
                        cachedValues.Remove(kv.Key);
                    }
                }
            }
            finally
            {
                readWriteCacheLock.ReleaseWriterLock();
            }

            // push the update out to all
            NotifyEventListenersAndSubscriberOfKeyValueUpdate(update);
        }

        private void NotifyEventListenersAndSubscriberOfKeyValueUpdate(KeyValueUpdate update)
        {
            // now inform all interested components of the value change
            // outside write lock to avoid lockups
            foreach (var kv in update.Item)
            {
                SendKeyChangedEvent(kv);
                if (kv.Value != null)
                {
                    SendKeyChangedEventToSubscribers(kv);
                }
                else
                {
                    SendKeyDeletedEventToDeleteSubscribers(kv);
                }
            }
        }

        private void SendKeyChangedEvent(KeyValue kv)
        {
            try
            {
                KeyChanged(kv.Key);
                KeyValueChanged(kv);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception thrown by event listeners of key-value changes in room {roomName} for key {kv.Key} with exception {ex}");
            }
        }

        private void SendKeyChangedEventToSubscribers(KeyValue kv)
        {
            if (subscribers.ContainsKey(kv.Key))
            {
                subscribers[kv.Key].ForEach(sub =>
                {
                    try
                    {
                        sub(kv.Value.ToByteArray());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception thrown by subscriber of key-value changes in room {roomName} for key {kv.Key} with exception {ex}");
                    }
                });
            }
        }

        private void SendKeyDeletedEventToDeleteSubscribers(KeyValue kv)
        {
            if (deleteSubscribers.ContainsKey(kv.Key))
            {
                deleteSubscribers[kv.Key].ForEach(sub =>
                {
                    try
                    {
                        sub();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception thrown by delete subscriber of key-value deletions in room {roomName} for key {kv.Key} with exception {ex}");
                    }
                });
            }
        }

        IEnumerator<KeyValuePair<string, byte[]>> IEnumerable<KeyValuePair<string, byte[]>>.GetEnumerator()
        {
            readWriteCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                return cachedValues.ToList().GetEnumerator();
            }
            finally
            {
                readWriteCacheLock.ReleaseReaderLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            readWriteCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                return cachedValues.ToList().GetEnumerator();
            }
            finally
            {
                readWriteCacheLock.ReleaseReaderLock();
            }
        }

    }
}
