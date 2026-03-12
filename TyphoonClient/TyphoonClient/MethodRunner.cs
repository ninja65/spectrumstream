////////////////////////////////////////////////////////////////////////////
//
// Copyright � 2013-2015 Waters Corporation.
//
// Method runner implementation by typhoon client to provide
// method running functionality from typhoon.
//
////////////////////////////////////////////////////////////////////////////

using System;
using Waters.Control.Client.Interface;
using Waters.Control.Client.InternalInterface;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    /// <summary>
    /// Method runner implementation
    /// </summary>
    public class MethodRunner : IMethodRunner, IDisposable
    {
        private IClientAccess client;

        /// <summary>
        /// Submitted method status event
        /// </summary>
        public event Action<MethodEvent, MSMethodDetails> MethodEvent = (me, details) => { };
        public event Action MethodWaitingForTrigger = () => { };
        public event Action MethodTriggered = () => { };
        public event Action MethodStarted = () => { };
        public event Action MethodComplete = () => { };
        public event Action MethodAborted = () => { };
        public event Action MethodSettling = () => { };
        public event Action MethodSettled = () => { };
        public event Action MethodSettleTimeout = () => { };
        public event Action MethodError = () => { };

        /// <summary>
        /// Event containing function details of completed function
        /// </summary>
        public event Action<MethodFunctionKeyMapItem> FunctionComplete = fc => { };

        /// <summary>
        /// Event containing instrument settings for method report.
        /// </summary>
        public event Action<MethodReportSettings> MethodReport = mr => { };

        /// <summary>
        /// Scan data event
        /// </summary>
        public event Action<ScanData> ScanDataEvent = d => { };

        /// <summary>
        /// Tracked Parameter Update Event
        /// </summary>
        public event Action<TrackedParameterUpdate> TrackedParameterUpdateEvent = t => { };


        /// <summary>
        /// Constructor with its client access injected
        /// </summary>
        /// <param name="client">The client access</param>
        public MethodRunner(IClientAccess client)
        {
            this.client = client;
            client.RegisterHandler<MSMethodDetails>("MethodRunner.Start", OnMethodStarted);
            client.RegisterHandler<MSMethodDetails>("MethodRunner.MethodWaitingForTrigger", OnMethodWaitingForTrigger);
            client.RegisterHandler<MSMethodDetails>("MethodRunner.MethodTriggered", OnMethodTriggered);
            client.RegisterHandler<MSMethodDetails>("MethodRunner.Complete", OnMethodComplete);
            client.RegisterHandler<MSMethodDetails>("MethodRunner.MethodAborted", OnMethodAborted);
            client.RegisterHandler<MSMethodDetails>("MethodRunner.MethodError", OnMethodError);
            client.RegisterHandler<MSMethodDetails>("MethodRunner.MethodSettling", OnMethodSettling);
            client.RegisterHandler<MSMethodDetails>("MethodRunner.MethodSettled", OnMethodSettled);
            client.RegisterHandler<MSMethodDetails>("MethodRunner.SettleTimeout", OnMethodSettleTimeout);
            client.RegisterHandler<MethodFunctionKeyMapItem>("MethodRunner.FunctionComplete", OnFunctionComplete);
            client.RegisterHandler<ScanData>("DataStreaming.DataPublish", OnScanDataEvent);
            client.RegisterHandler<TrackedParameterUpdate>("Pub.InstParams.TrackedUpdate", OnTrackedParameterUpdateEvent);
            client.RegisterHandler<MethodReportSettings>("MethodRunner.Report", OnMethodReportEvent);
        }

        
        /// <summary>
        /// Run supplied method
        /// </summary>
        /// <param name="msMethod">The method to run</param>
        public void RunMethod(MSMethod msMethod, string tag = null)
        {
            client.Request("MethodRunner", "MethodRunner.RunMethod", msMethod, tag);
        }

        /// <summary>
        /// Run supplied method
        /// </summary>
        /// <param name="msMethod">The method to run</param>
        /// <param name="tag">Locking tag</param>
        public void Run(MSMethod msMethod, string tag)
        {
            RunMethod(msMethod, tag);
        }

        /// <summary>
        /// Run supplied method
        /// </summary>
        /// <param name="method">The method to run</param>
        public void Run(MSMethod msMethod)
        {
            RunMethod(msMethod, null);
        }

        /// <summary>
        /// Signal the method runner to start running the method.
        /// This is used to start the method when method.extern_event=true, in this situation
        /// Typhoon gets ready to run the method then waits for a signal from the external controller
        /// (e.g. UNIFI Osprey driver) to contunue and execute the method
        /// </summary>
        public void DataSystemStart()
        {
            client.Request("MethodRunner", "MethodRunner.DataSystemStart");
        }

        /// <summary>
        /// Signal the method runner to GotoInitialConditions.
        /// </summary>
        public void GoToInitialConditions(MSMethod msMethod)
        {
            client.Request("MethodRunner", "MethodRunner.ApplyInitialConditions", msMethod);
        }

        /// <summary>
        /// Abort the current running method.
        /// </summary>
        public void Abort()
        {
            Abort(new MSMethodId());
        }

        /// <summary>
        /// Abort the method with given acquisition id.
        /// </summary>
        public void Abort(string acquisitionId)
        {
            Abort(new MSMethodId { AcquisitionId = acquisitionId});
        }

        /// <summary>
        /// Abort the method with given acquisition id.
        /// </summary>
        public void Abort(MSMethodId methodId)
        {
            client.Request("MethodRunner", "MethodRunner.Abort", methodId);
        }

        private void OnScanDataEvent(ScanData scanData)
        {
            ScanDataEvent(scanData);
        }

        private void OnTrackedParameterUpdateEvent(TrackedParameterUpdate trackedParameterUpdate)
        {
            TrackedParameterUpdateEvent(trackedParameterUpdate);
        }

        private void OnMethodStarted(MSMethodDetails details)
        {
            MethodStarted();
            MethodEvent(Interface.MethodEvent.Started, details);
        }

        private void OnMethodWaitingForTrigger(MSMethodDetails details)
        {
            MethodWaitingForTrigger();
            MethodEvent(Interface.MethodEvent.WaitingForTrigger, details);
        }

        private void OnMethodTriggered(MSMethodDetails details)
        {
            MethodTriggered();
            MethodEvent(Interface.MethodEvent.Triggered, details);
        }

        private void OnMethodComplete(MSMethodDetails details)
        {
            MethodComplete();
            MethodEvent(Interface.MethodEvent.Complete, details);
        }

        private void OnMethodAborted(MSMethodDetails details)
        {
            MethodAborted();
            MethodEvent(Interface.MethodEvent.Aborted, details);
        }

        private void OnMethodError(MSMethodDetails details)
        {
            MethodError();
            MethodEvent(Interface.MethodEvent.Error, details);
        }

        private void OnMethodSettling(MSMethodDetails details)
        {
            MethodSettling();
            MethodEvent(Interface.MethodEvent.Settling, details);
        }

        private void OnMethodSettled(MSMethodDetails details)
        {
            MethodSettled();
            MethodEvent(Interface.MethodEvent.Settled, details);
        }

        private void OnMethodSettleTimeout(MSMethodDetails details)
        {
            MethodSettleTimeout();
            MethodEvent(Interface.MethodEvent.SettleTimeout, details);
        }

        private void OnMethodReportEvent(MethodReportSettings data)
        {
            MethodReport(data);
        }

        private void OnFunctionComplete(MethodFunctionKeyMapItem functionItem)
        {
            FunctionComplete(functionItem);
        }

        public void Dispose()
        {
            client.UnregisterHandler<MSMethodDetails>("MethodRunner.MethodWaitingForTrigger", OnMethodWaitingForTrigger);
            client.UnregisterHandler<MSMethodDetails>("MethodRunner.MethodTriggered", OnMethodTriggered);
            client.UnregisterHandler<MSMethodDetails>("MethodRunner.Start", OnMethodStarted);
            client.UnregisterHandler<MSMethodDetails>("MethodRunner.Complete", OnMethodComplete);
            client.UnregisterHandler<MSMethodDetails>("MethodRunner.MethodAborted", OnMethodAborted);
            client.UnregisterHandler<MSMethodDetails>("MethodRunner.MethodError", OnMethodError);
            client.UnregisterHandler<MSMethodDetails>("MethodRunner.MethodSettling", OnMethodSettling);
            client.UnregisterHandler<MSMethodDetails>("MethodRunner.MethodSettled", OnMethodSettled);
            client.UnregisterHandler<MSMethodDetails>("MethodRunner.SettleTimeout", OnMethodSettleTimeout);
            client.UnregisterHandler<MethodFunctionKeyMapItem>("MethodRunner.FunctionComplete", OnFunctionComplete);
            client.UnregisterHandler<ScanData>("DataStreaming.DataPublish", ScanDataEvent);
            client.UnregisterHandler<TrackedParameterUpdate>("Pub.InstParams.TrackedUpdate", TrackedParameterUpdateEvent);
            client.UnregisterHandler<MethodReportSettings>("MethodRunner.Report", OnMethodReportEvent);
            client = null;
        }

    }
}
