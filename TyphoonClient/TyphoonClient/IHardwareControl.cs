////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2013-2019 Waters Corporation.
//
// IHardwareControl interface - provides access to set the instrument
// parameters on the instrument via Typhoon system.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Waters.Control.Message;

namespace Waters.Control.Client.Interface
{
    /// <summary>
    /// Instrument hardware control interface
    /// </summary>
    public interface IHardwareControl
    {
        /// <summary>
        /// Event fired when a <see cref="ParameterValue"/> has changed.
        /// </summary>
        event Action<ParameterValue> ParameterChange;

        /// <summary>
        /// Event fired when a <see cref="ModeValue"/> has changed.
        /// </summary>
        event Action<ModeValue> ModeChange;

        /// <summary>
        /// Event fired when the online state has changed.
        /// </summary>
        //event Action<bool> OnlineEvent;

        /// <summary>
        /// Returns true if the instrument is online.
        /// </summary>
        bool IsOnline { get; }

        /// <summary>
        /// Set the instrument parameter value.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        void Set(string parameterName, VariantValue value);

        /// <summary>
        /// Set the instrument parameter value.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <param name="value"></param>
        void Set(string parameterName, double value);

        /// <summary>
        /// Set the instrument parameter value.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <param name="value"></param>
        void Set(string parameterName, string value);

        /// <summary>
        /// Get the current value of the specified instrument parameter.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <returns></returns>
        [System.Obsolete("Method is deprecated, please use GetSetting() instead.")]
        double Get(string parameterName);

        /// <summary>
        /// Get the current value of the specified instrument parameter.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <returns></returns>
        VariantValue GetSetting(string parameterName);

        /// <summary>
        /// Returns true if the specified instrument parameter exists.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <returns></returns>
        bool Exists(string parameterName);

        /// <summary>
        /// Get the definition of the specified instrument parameter.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <returns></returns>
        InstParamAttribs GetInstrumentParameterConfig(string parameterName);

        /// <summary>
        /// Get the current value of the specified instrument parameter for the specified set of instrument modes.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <param name="modes">List of matching modes; for example to get the corresponding instrument config value for Positive Sensitivity polarity and analyzer mode.</param>
        /// <returns></returns>
        VariantValue GetInstrumentParameterValue(string parameterName, IList<InstrumentMode> modes);

        /// <summary>
        /// Get the current value of the specified instrument config parameter for the specified set of instrument modes.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <param name="modes">List of matching modes; for example to get the corresponding instrument config value for Positive Sensitivity polarity and analyzer mode.</param>
        /// <returns></returns>
        VariantValue GetInstrumentParameterValue(string parameterName, params InstrumentMode[] modes);

        /// <summary>
        /// Get the instrument parameters
        /// </summary>
        IEnumerable<ParameterValue> GetInstrumentParameters();

        /// <summary>
        /// Apply a list of initial conditions
        /// </summary>
        void PushInitialConditions(IEnumerable<ParameterValue> parameters);

        /// <summary>
        /// Apply a list of initial conditions
        /// </summary>
        void PushInitialConditions(IEnumerable<ModeValue> modes);

        /// <summary>
        /// Apply a list of initial conditions
        /// </summary>
        void PushInitialConditions(IEnumerable<ParameterValue> parameters, IEnumerable<ModeValue> modes);

        /// <summary>
        /// Pop the last pushed initial conditions
        /// </summary>
        void PopInitialConditions();

        /// <summary>
        /// Set the mode to the specified value.
        /// </summary>
        /// <param name="modeName">Mode name.</param>
        /// <param name="modeValue">Mode value.</param>
        void SetMode(string modeName, string modeValue);

        /// <summary>
        /// Get the current value of the specified mode.
        /// </summary>
        /// <param name="modeName"></param>
        /// <returns></returns>
        string GetMode(string modeName);

        /// <summary>
        /// Returns true if the specified instrument mode exists.
        /// </summary>
        /// <param name="modeName">Parameter/key name; for example "OpticMode".</param>
        /// <returns></returns>
        bool ModeExists(string modeName);

        /// <summary>
        /// Get all the instrument modes with current state
        /// </summary>
        IEnumerable<ModeValue> GetInstrumentModes();

        /// <summary>
        /// Get the definition of the specified instrument mode.
        /// </summary>
        /// <param name="modeName">Parameter/key name; for example "OpticMode".</param>
        /// <returns></returns>
        InstrumentModeSet GetModeConfig(string modeName);

        /// <summary>
        /// Load Factory Settings.
        /// </summary>
        void LoadFactorySetting();

        /// <summary>
        /// Save Factory Settings.
        /// </summary>
        void SaveFactorySetting();

        /// <summary>
        /// Reset to Factory Settings.
        /// </summary>
        void ResetToFactorySettings();

        /// <summary>
        /// Reset to Default Settings
        /// </summary>
        void ResetToDefaultSettings();

        /// <summary>
        /// Set the instrument into the operate state.
        /// </summary>
        void Operate();

        /// <summary>
        /// Disable the delayed settling
        /// </summary>
        void DisableDelayedSettling();

        /// <summary>
        /// Event fired when factory setting save has completed.
        /// </summary>
        event Action FactorySettingSavedComplete;
    }
}
