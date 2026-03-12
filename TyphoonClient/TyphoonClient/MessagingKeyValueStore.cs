using Waters.Control.Client.InternalInterface;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    public class MessagingKeyValueStore : KeyValueStore
    {
        private readonly IClientAccess clientAccess;
        private readonly IClientConnector clientConnector;

        public MessagingKeyValueStore(IClientAccess clientAccess, IClientConnector clientConnector)
        {
            this.clientAccess = clientAccess;
            this.clientConnector = clientConnector;
        }

        protected override bool IsConnected { get { return clientConnector.IsConnected; } }

        public override KeyValueRoom Open(string roomName, KeyValueRoom.Flags flags)
        {
            lock (LockObject)
            {
                // open the room in the cache
                var room = base.Open(roomName, flags);

                // register the room for message updates
                clientAccess.RegisterHandler("KeyValueStore." + roomName + ".Update",
                    (KeyValueUpdate update) => OnUpdate(update));

                return room;
            }
        }

        public override KeyValueUpdate SyncRoom(string roomName, KeyValueRoom.Flags flags)
        {
            var syncRequest = new KeyValueStoreSyncRequest { Room = roomName, Persist = (flags == KeyValueRoom.Flags.Persisted) };

            var reply = clientAccess.RequestReply("KeyValueStore", "KeyValueStore.SyncRequest", syncRequest);
            return reply.Message<KeyValueUpdate>();
        }

        public override void SyncPut(KeyValueStorePut putUpdate)
        {
            clientAccess.Request("KeyValueStore", "KeyValueStore.SyncPut", putUpdate);
        }

        public override void SyncClear(KeyValueStoreClear clearUpdate)
        {
            clientAccess.Request("KeyValueStore", "KeyValueStore.SyncClear", clearUpdate);
        }
    }
}
