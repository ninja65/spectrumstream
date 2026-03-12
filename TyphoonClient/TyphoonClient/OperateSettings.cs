////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2019 Waters Corporation.
//
// Constants values for Operate Instrument settings 
//
////////////////////////////////////////////////////////////////////////////

namespace Waters.Control.Client.Interface
{
    /// <summary>
    /// OperateSettings
    /// </summary>
    public enum OperateSettings
    {
        // Note: these values should match the correspondent values 
        // from the instrument settings json file

        // values for: 
        // Operate.Readback
        Standby,    // instrument standby
        PowerSave,  // is Source Standby on Osprey
        Operate,

        // from Osprey RIO
        RFTrip
    }
}
