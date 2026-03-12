using System.Collections.Generic;
using System.Linq;
using Waters.Control.Client.InternalInterface;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    public abstract class KeyValueStore : IKeyValueStore
    {
        protected readonly Dictionary<string, KeyValueRoom> rooms = new Dictionary<string, KeyValueRoom>();
        protected readonly object LockObject = new object();

        protected virtual bool IsConnected { get; }

        public IEnumerable<string> Rooms
        {
            get
            {
                return rooms.Keys.ToList();
            }
        }

        public IKeyValueRoom OpenRoom(string roomName)
        {
            return Open(roomName);
        }

        public KeyValueRoom Open(string roomName)
        {
            return Open(roomName, KeyValueRoom.Flags.InMemory);
        }

        public IKeyValueRoom OpenRoom(string roomName, KeyValueRoom.Flags flags)
        {
            return Open(roomName, flags);
        }

        public virtual KeyValueRoom Open(string roomName, KeyValueRoom.Flags flags)
        {
            // only allow one room opener at a time
            lock (LockObject)
            {
                KeyValueRoom room = null;
                if (!rooms.ContainsKey(roomName))
                {
                    // open a new room
                    room = new KeyValueRoom(this, roomName, flags);
                    rooms[roomName] = room;

                    // if there is currently a connection to typhoon then resync the room
                    // to initialise its key-values, if not connected then this will
                    // be performed for all rooms when the connection is setup
                    if (IsConnected) room.Resync();
                }
                else
                {
                    // room already open, let the room check if the flags have changed
                    room = rooms[roomName];
                    room.UpdateFlags(flags);
                }

                return room;
            }
        }

        public IKeyValueRoom OpenPersistedRoom(string roomName)
        {
            return OpenPersisted(roomName);
        }

        public KeyValueRoom OpenPersisted(string roomName)
        {
            return Open(roomName, KeyValueRoom.Flags.Persisted);
        }

        public void ClearAll(bool close = false)
        {
            lock (LockObject)
            {
                var clearUpdate = new KeyValueStoreClear
                {
                    ClearAll = true,
                    Close = close
                };
                SyncClear(clearUpdate);
                if (close)
                {
                    rooms.Clear();
                }
            }
        }

        protected void OnUpdate(KeyValueUpdate update)
        {
            if (rooms.ContainsKey(update.Room))
            {
                rooms[update.Room].OnUpdate(update);
            }
        }

        public abstract KeyValueUpdate SyncRoom(string roomName, KeyValueRoom.Flags flags);
        public abstract void SyncPut(KeyValueStorePut putUpdate);
        public abstract void SyncClear(KeyValueStoreClear putUpdate);

        /// <summary>
        /// Resync rooms
        /// </summary>
        public void Reset()
        {
            // now iterate through the rooms resyncing them
            var roomList = rooms.Values.ToList();
            foreach (var room in roomList)
            {
                room.Resync();
            }
        }
    }
}

