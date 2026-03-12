////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2013-2018 Waters Corporation.
//
// Method status event.
//
////////////////////////////////////////////////////////////////////////////


namespace Waters.Control.Client.Interface
{
    /// <summary>
    /// Running method status event
    /// </summary>
    public enum MethodEvent
    {
        /// <summary>
        /// WaitingForTrigger
        /// </summary>
        WaitingForTrigger,

        /// <summary>
        /// Triggered
        /// </summary>
        Triggered,

        /// <summary>
        /// Started
        /// </summary>
        Started,

        /// <summary>
        /// Complete
        /// </summary>
        Complete,

        /// <summary>
        /// Aborted
        /// </summary>
        Aborted,

        /// <summary>
        /// Error
        /// </summary>
        Error,

        /// <summary>
        /// Settled
        /// </summary>
        Settled,

        /// <summary>
        /// Settling
        /// </summary>
        Settling,

        /// <summary>
        /// SettleTimeout
        /// </summary>
        SettleTimeout
    };
}
