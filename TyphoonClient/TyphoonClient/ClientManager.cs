////////////////////////////////////////////////////////////////////////////
// 
// Copyright © 2018 Waters Corporation.
// 
// Access to Client Manager Service functionality
// 
////////////////////////////////////////////////////////////////////////////
using System;
using System.Threading.Tasks;
using Waters.Control.Client.Interface;
using Waters.Control.Client.InternalInterface;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    /// <summary>
    /// Wrapper class to Client Manager Service functionality
    /// </summary>
    public class ClientManager : IClientManager
    {
        private readonly IClientAccess clientAccess;
        private readonly TimeSpan timeout;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientAccess"></param>
        public ClientManager(IClientAccess clientAccess, TimeSpan timeout)
        {
            this.clientAccess = clientAccess;
            this.timeout = timeout;
        }

        public ClientManager(IClientAccess clientAccess)
            : this(clientAccess, TimeSpan.FromSeconds(5))
        {
        }
        /// <summary>
        /// Request the client manager lock
        /// </summary>
        /// <param name="lockerTag"></param>
        /// <returns></returns>
        public bool RequestLock(string lockerTag)
        {
            return PerformLockRequest("ClientManager.RequestLock", LockResult.Types.LockState.Locked, lockerTag);
        }
        /// <summary>
        /// Refresh the client manager lock
        /// </summary>
        /// <param name="lockerTag"></param>
        /// <returns></returns>
        public bool RefreshLock(string lockerTag)
        {
            return PerformLockRequest("ClientManager.RefreshLock", LockResult.Types.LockState.Locked, lockerTag);
        }
        /// <summary>
        /// Release the client manager lock
        /// </summary>
        /// <param name="lockerTag"></param>
        public bool ReleaseLock(string lockerTag)
        {
            return PerformLockRequest("ClientManager.ReturnLock", LockResult.Types.LockState.Unlocked, lockerTag);
        }


        private bool PerformLockRequest(string lockRequestMessage, LockResult.Types.LockState expectedResultState, string lockerTag)
        {
            bool success = false;
            TaskCompletionSource<bool> requestLockTask = new TaskCompletionSource<bool>();
            Action<LockResult> requestLockResultAction = (lr) =>
            {
                if (lr.Tag == lockerTag)
                {
                    requestLockTask.SetResult(lr.State == expectedResultState);
                }
            };
            clientAccess.RegisterHandler<LockResult>("ClientManager.LockResult", requestLockResultAction);
            try
            {
                clientAccess.Request<LockRequest>("ClientManager", lockRequestMessage, new LockRequest() { Tag = lockerTag });
                // wait for the result of the lock request
                if (requestLockTask.Task.Wait(timeout))
                {
                    success = requestLockTask.Task.Result;
                }
                else
                {
                    Console.WriteLine("Error time out waiting for lock request to complete");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error - Exception thrown requesting {lockRequestMessage}:" + ex);
            }
            finally
            {
                clientAccess.UnregisterHandler<LockResult>("ClientManager.LockResult", requestLockResultAction);
            }
            return success;
        }

    }
}
