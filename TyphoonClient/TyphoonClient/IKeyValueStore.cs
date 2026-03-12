
using System.Collections.Generic;
using Waters.Control.Message;
namespace Waters.Control.Client.InternalInterface
{
    /// <summary>
    ///
    /// </summary>
    public interface IKeyValueStore
    {
        IEnumerable<string> Rooms { get; }
        /// <summary>
        ///
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns></returns>
        IKeyValueRoom OpenRoom(string roomName);
        /// <summary>
        ///
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        IKeyValueRoom OpenRoom(string roomName, KeyValueRoom.Flags flags);
        /// <summary>
        ///
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns></returns>
        IKeyValueRoom OpenPersistedRoom(string roomName);
        /// <summary>
        ///
        /// </summary>
        /// <param name="close"></param>
        void ClearAll(bool close);
        /// <summary>
        ///
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        KeyValueUpdate SyncRoom(string roomName, KeyValueRoom.Flags flags);
        /// <summary>
        ///
        /// </summary>
        /// <param name="putUpdate"></param>
        void SyncPut(KeyValueStorePut putUpdate);
        /// <summary>
        ///
        /// </summary>
        /// <param name="putUpdate"></param>
        void SyncClear(KeyValueStoreClear putUpdate);
        /// <summary>
        ///
        /// </summary>
        void Reset();
    }
}
