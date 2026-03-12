namespace Waters.Control.Client.Interface
{
    /// <summary>
    /// Possible status of the connection between [Typhoon] - [UNIFI MS instrument driver/Typhon-Client]
    /// </summary>
    public enum TyphoonConnectionStatus
    {
        /// <summary>
        /// Unrecognised status - probably not detected yet
        /// </summary>
        UNKNOWN = 0,

        /// <summary>
        /// Connection succeded
        /// </summary>
        SUCCEEDED = 1,

        /// <summary>
        /// Connection failed
        /// </summary>
        FAILED = 2,

        /// <summary>
        /// Idle status  - TODO not in use yet
        /// </summary>
        IDLE = 3,

        /// <summary>
        /// Error   - TODO not in use yet
        /// </summary>
        ERROR = 4
    }

}
