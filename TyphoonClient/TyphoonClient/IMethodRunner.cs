////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2013-2015 Waters Corporation.
//
// Method runner interface implemented by typhoon client to provide
// method running functionality from typhoon.
//
////////////////////////////////////////////////////////////////////////////

using System;
using Waters.Control.Message;

namespace Waters.Control.Client.Interface
{
    /// <summary>
    /// Method runner interface
    /// </summary>
    public interface IMethodRunner
    {
        /// <summary>
        /// Submitted method status event
        /// </summary>       
        event Action<MethodEvent, MSMethodDetails> MethodEvent;
        event Action MethodStarted;
        event Action MethodComplete;
        event Action MethodSettled;
        event Action MethodError;
        event Action MethodAborted;

        /// <summary>
        /// Event containing function details of completed function
        /// </summary>
        event Action<MethodFunctionKeyMapItem> FunctionComplete;

        /// <summary>
        /// Event containing instrument settings for method report.
        /// </summary>
        event Action<MethodReportSettings> MethodReport;

        /// <summary>
        /// Scan data event
        /// </summary>
        event Action<ScanData> ScanDataEvent;

        /// <summary>
        /// Tracked Parameter Update event
        /// </summary>
        event Action<TrackedParameterUpdate> TrackedParameterUpdateEvent;

        /// <summary>
        /// Run supplied method
        /// </summary>
        /// <param name="method">The method to run</param>
        void Run(MSMethod method);

        /// <summary>
        /// Run supplied method
        /// </summary>
        /// <param name="method">The method to run</param>
        /// <param name="tag">Locker tag</param>
        void Run(MSMethod msMethod, string tag);

        /// <summary>
        /// Abort the current running method.
        /// </summary>
        void Abort();

        /// <summary>
        /// Abort the method with given acquisition id.
        /// </summary>
        void Abort(string acquisitionId);

        /// <summary>
        /// Abort the method with given acquisition id.
        /// </summary>
        void Abort(MSMethodId methodId);

        /// <summary>
        /// Send system start to MethodRunner to continue a waiting method due to MethodOptions.wait_for_datasystem in method
        /// </summary>
        void DataSystemStart();

        /// <summary>
        /// Send system to Goto Initial Conditions
        /// </summary>
        void GoToInitialConditions(MSMethod method);
    }
}