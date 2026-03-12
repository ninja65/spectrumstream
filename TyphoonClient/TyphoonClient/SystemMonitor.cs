////////////////////////////////////////////////////////////////////////////
// 
// Copyright © 2013-2015 Waters Corporation.
// 
// Monitor the typhoon connection listening for the heart beat to determine
//  if Typhoon is still alive or whether it has been restarted
// 
////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Waters.Control.Client.Interface;
using Waters.Control.Client.InternalInterface;
using Waters.Control.Message;
namespace Waters.Control.Client
{
    /// <summary>
    /// Monitor the typhoon connection listening for the heart beat to determine
    ///  if Typhoon is still alive or whether it has been restarted
    /// </summary>
    public class SystemMonitor : ISystemMonitor
    {
        private readonly TaskScheduler taskScheduler;
        /// <summary>
        /// Status event to signal if Typhoon is alive or dead or restarted
        /// </summary>
        public event Action<SystemStatus> Status = (s) => { };

        private Timer timer;
        private readonly Stopwatch stopwatch;
        private SystemStatus currentHeartBeatStatus = SystemStatus.OK;
        private long timeoutIntervalInMs;
        private string lastSenderId;
        private readonly object lockObject = new object();
        private bool restartSignalled;
        private IClientAccess clientAccess;
        private readonly bool startMonitoring;
        private readonly int checkForHeartBeatIntervalInMs;


        /// <summary>
        /// Typhoon Timeout Interval in seconds
        /// </summary>
        public double TimeoutInterval
        {
            set
            {
                timeoutIntervalInMs = (long)(1000 * value);
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="clientAccess"></param>
        public SystemMonitor(TaskScheduler taskScheduler, IClientAccess clientAccess)
            : this(taskScheduler, clientAccess, 10000, 500, true)
        {
        }

        /// <summary>
        /// Parameter constructor
        /// </summary>
        /// <param name="clientAccess">Client access</param>
        /// <param name="timeoutIntervalInMs">Time in ms to wait for no response from Typhoon to consider it inactive</param>
        /// <param name="checkForHeartBeatIntervalInMs">Time in ms to repeatedly check whether Typhoon is alive</param>
        /// <param name="startMonitoring">Start monitoring, only used for preventing the timer from being started in unit tests</param>
        /// <param name="initialStatus">Initial state of connection</param>
        public SystemMonitor(TaskScheduler taskScheduler, IClientAccess clientAccess, long timeoutIntervalInMs, int checkForHeartBeatIntervalInMs, bool startMonitoring = true, SystemStatus initialStatus = SystemStatus.OK)
        {
            this.taskScheduler = taskScheduler;
            this.clientAccess = clientAccess;
            this.timeoutIntervalInMs = timeoutIntervalInMs;
            currentHeartBeatStatus = initialStatus;
            this.startMonitoring = startMonitoring;
            this.checkForHeartBeatIntervalInMs = checkForHeartBeatIntervalInMs;

            stopwatch = new Stopwatch();
        }

        public void StartMonitoringConnection()
        {
            clientAccess.RegisterHandler<HeartBeatInfo>("HeartBeat", Beat);
            timer = new Timer((o) => CheckHeartBeat(), null, checkForHeartBeatIntervalInMs, checkForHeartBeatIntervalInMs);

            if (startMonitoring)
            {
                stopwatch.Start();
            }
        }
        /// <summary>
        /// Publish the current state
        /// </summary>
        public void PublishSystemStatus()
        {
            var status = currentHeartBeatStatus;
            Task.Factory.StartNew(
              () =>
              {
                  try
                  {
                      SendStatus(status);
                  }
                  catch (Exception ex)
                  {
                      Console.WriteLine($"Exception publishing system status in system monitor:{ex}");
                  }
              },
              new CancellationToken(),
              TaskCreationOptions.None,
              taskScheduler);
        }

        /// <summary>
        /// Get the status.
        /// </summary>
        /// <returns>Status</returns>
        public SystemStatus GetStatus()
        {
            return currentHeartBeatStatus;
        }

        /// <summary>
        /// Called when a heart beat is received from Typhoon
        /// </summary>
        /// <param name="info"></param>
        internal void Beat(HeartBeatInfo info)
        {
            if (lastSenderId == null)
            {
                lastSenderId = info.SenderInstanceId;
            }

            // start the stopwatch from this beat
            stopwatch.Restart();

            lock (lockObject)
            {
                // if the sender id has changed means that Typhoon has been restarted
                if (lastSenderId != info.SenderInstanceId)
                {
                    Console.WriteLine("System Monitor received a heart beat with a different sender id, assuming Typhoon has restarted");

                    lastSenderId = info.SenderInstanceId;
                    currentHeartBeatStatus = SystemStatus.Restarted;
                    restartSignalled = true;

                    // send the restart signal now on the beat thread so it is not missed
                    SendStatus(SystemStatus.Restarted);
                }
                else if (restartSignalled)
                {
                    // in here when the next beat after a restart beat has occurred
                    restartSignalled = false;
                }
            }
        }

        /// <summary>
        /// Called on timer thread at intervals to check if the beat is overdue
        /// </summary>
        internal void CheckHeartBeat()
        {
            lock (lockObject)
            {
                if (currentHeartBeatStatus == SystemStatus.OK || restartSignalled == true)
                {
                    // if the beat is overdue send a NoResponse event i.e. RIP Typhoon it is dead.
                    if (stopwatch.ElapsedMilliseconds >= timeoutIntervalInMs)
                    {
                        Console.WriteLine("System Monitor has not received heart beat for a period of time assuming Tyhoon is down");
                        currentHeartBeatStatus = SystemStatus.NoResponse;
                        restartSignalled = false;
                        Status(SystemStatus.NoResponse);
                    }
                }
                else
                {
                    // when we hear the heartbeat after no response send an ok
                    // for restarts wait till we have an ok beat before considering connection is ok
                    if (stopwatch.ElapsedMilliseconds < timeoutIntervalInMs && restartSignalled == false)
                    {
                        Console.WriteLine("System Monitor received heart beat after disconnection");
                        currentHeartBeatStatus = SystemStatus.OK;
                        Status(SystemStatus.OK);
                    }
                }
            }
        }

        /// <summary>
        /// Wait for the next good heart beat
        /// </summary>
        /// <returns>Returns true for beat received, false for timeout</returns>
        public bool WaitForHeartbeat()
        {
            bool beatReceived;

            // wait for the first heart beat from typhoon so we know it is a stable connection
            TaskCompletionSource<bool> beated = new TaskCompletionSource<bool>();
            Action<SystemStatus> heartbeatReceivedAction = (beatStatus) =>
            {
                if (beatStatus == SystemStatus.OK) beated.TrySetResult(true);
            };
            Status += heartbeatReceivedAction;
            try
            {
                Task.Factory.StartNew(
                  () => heartbeatReceivedAction(GetStatus()),
                  new CancellationToken(),
                  TaskCreationOptions.None,
                  taskScheduler);
                // wait for the first heart beat or the timeout period
                beatReceived = beated.Task.Wait(TimeSpan.FromMilliseconds(timeoutIntervalInMs));
            }
            finally
            {
                Status -= heartbeatReceivedAction;
            }

            // return whether the heartbeat was received
            return beatReceived;
        }

        private void SendStatus(SystemStatus status)
        {
            try
            {
                Status(status);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception sending System Monitor status" + ex);
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (clientAccess != null)
            {
                clientAccess.UnregisterHandler<HeartBeatInfo>("HeartBeat", Beat);
                clientAccess = null;
            }

            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

    }
}