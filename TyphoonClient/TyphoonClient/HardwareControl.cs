////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2013-2019 Waters Corporation.
//
// HardwareControl class - provides access to set the instrument
// parameters on the instrument via Typhoon system.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using Waters.Control.Client.Interface;
using Waters.Control.Client.InternalInterface;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    /// <summary>
    /// HardwareControl
    /// </summary>
    public class HardwareControl : IHardwareControl, IDisposable
    {
        private IClientAccess clientAccess;
        private IKeyValueRoom instrumentParameter;
        private IKeyValueRoom modes;
        private IKeyValueRoom mainRoom;

        /// <summary>
        /// Event fired when a <see cref="ParameterValue"/> has changed.
        /// </summary>
        public event Action<ParameterValue> ParameterChange = e => { };

        /// <summary>
        /// Event fired when a <see cref="ModeValue"/> has changed.
        /// </summary>
        public event Action<ModeValue> ModeChange = e => { };

        /// <summary>
        /// Event fired when the online state has changed.
        /// </summary>
        //public event Action<bool> OnlineEvent = e => { };

        /// <summary>
        /// Returns true if the instrument is online.
        /// </summary>
        public bool IsOnline { get; private set; }

        /// <summary>
        /// Event fired when factory setting save has completed.
        /// </summary>
        public event Action FactorySettingSavedComplete = () => { };

        /// <summary>
        /// Initializes a new instance of the <see cref="HardwareControl"/> class.
        /// </summary>
        /// <param name="clientAccess"></param>
        /// <param name="keyValueStore"></param>
        public HardwareControl(IClientAccess clientAccess, IKeyValueStore keyValueStore)
        {
            this.clientAccess = clientAccess;
            this.instrumentParameter = keyValueStore.OpenRoom("HardwareControl.InstrumentParameters");
            this.modes = keyValueStore.OpenRoom("HardwareControl.Modes");
            this.mainRoom = keyValueStore.OpenRoom("HardwareControl");
            instrumentParameter.KeyChanged += OnParametersRoomChange;
            modes.KeyChanged += OnModesRoomChange;
            //mainRoom.Subscribe<string>("Online", o => OnOnline(o));

            this.clientAccess.RegisterHandler("HardwareControl.FactorySettingSaved", FireFactorySettingSavedComplete);
        }


        /// <summary>
        /// Set the string type instrument parameter value.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        public void Set(string parameterName, VariantValue value)
        {
            var request = new SetInstrumentParameter
            {
                Name = parameterName,
                Value = value
            };
            clientAccess.Request("HardwareControl", "HardwareControl.SetSetting", request);
        }

        /// <summary>
        /// Set the instrument parameter value.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <param name="value"></param>
        public void Set(string parameterName, double value)
        {
            var variantValue = new VariantValue
            {
                DoubleValue = value
            };
            Set(parameterName, variantValue);
        }

        /// <summary>
        /// Set the instrument parameter value.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <param name="value"></param>
        public void Set(string parameterName, string value)
        {
            var variantValue = new VariantValue
            {
                StringValue = value
            };
            Set(parameterName, variantValue);
        }

        /// <summary>
        /// Set the current value of the specified instrument parameter.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <returns></returns>
        public double Get(string parameterName)
        {
            return instrumentParameter.Get<CurrentInstrumentParameter>(parameterName).Value.DoubleValue;
        }

        /// <summary>
        /// Get the current value of the specified instrument parameter.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <returns></returns>
        public VariantValue GetSetting(string parameterName)
        {
            return instrumentParameter.Get<CurrentInstrumentParameter>(parameterName).Value;
        }

        /// <summary>
        /// Returns true if the specified instrument parameter exists.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <returns></returns>
        public bool Exists(string parameterName)
        {
            return instrumentParameter.Exists(parameterName);
        }

        /// <summary>
        /// Get the definition of the specified instrument parameter.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <returns></returns>
        public InstParamAttribs GetInstrumentParameterConfig(string parameterName)
        {
            return instrumentParameter.Get<CurrentInstrumentParameter>(parameterName).Config;
        }

        /// <summary>
        /// Get the current value of the specified instrument parameter for the specified set of instrument modes.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <param name="modeSet">List of matching modes; for example to get the corresponding instrument config value for Positive Sensitivity polarity and analyzer mode.</param>
        /// <returns></returns>
        public VariantValue GetInstrumentParameterValue(string parameterName, IList<InstrumentMode> modeSet)
        {
            if (modeSet.Count == 0)
            {
                return GetSetting(parameterName);
            }

            var possibleModeSets = instrumentParameter.Get<CurrentInstrumentParameter>(parameterName).ModesValueList;

            foreach (var set in possibleModeSets)
            {
                bool setEqual = true;
                foreach (var mode in set.Modes)
                {
                    if (!modeSet.Any(p => (mode.Name == p.Name) && (mode.Value == p.Value)))
                    {
                        setEqual = false;
                        break;
                    }
                }

                if (setEqual)
                {
                    return set.Value;
                }
            }

            throw new ArgumentOutOfRangeException(parameterName + " not found in desired mode set");
        }

        /// <summary>
        /// Get the current value of the specified instrument config parameter for the specified set of instrument modes.
        /// </summary>
        /// <param name="parameterName">Parameter/key name; for example "Source.SourceTemperature.Setting".</param>
        /// <param name="modes">List of matching modes; for example to get the corresponding instrument config value for Positive Sensitivity polarity and analyzer mode.</param>
        /// <returns></returns>
        public VariantValue GetInstrumentParameterValue(string parameterName, params InstrumentMode[] modes)
        {
            return GetInstrumentParameterValue(parameterName, (IList<InstrumentMode>)modes);
        }

        /// <summary>
        /// Get the instrument parameters
        /// </summary>
        public IEnumerable<ParameterValue> GetInstrumentParameters()
        {
            foreach (var key in instrumentParameter.GetKeys())
            {
                var parameterValue = new ParameterValue
                {
                    Name = key,
                    Value = GetSetting(key)
                };
                yield return parameterValue;
            }
        }

        public void PushInitialConditions(IEnumerable<ParameterValue> parameters)
        {
            PushInitialConditions(parameters, new ModeValue[0]);
        }

        public void PushInitialConditions(IEnumerable<ModeValue> modes)
        {
            PushInitialConditions(new ParameterValue[0], modes);
        }

        public void PushInitialConditions(IEnumerable<ParameterValue> parameters, IEnumerable<ModeValue> modes)
        {
            var message = new InitialConditions();
            foreach (var parameter in parameters)
            {
                message.InitialCondition.Add(new InstrumentSetting { Name = parameter.Name, Value = parameter.Value });
            }

            foreach (var mode in modes)
            {
                message.Mode.Add(new InstrumentMode { Name = mode.Name, Value = mode.State });
            }

            clientAccess.Request("HardwareControl", "HardwareControl.PushInitialConditions", message);
        }

        public void PopInitialConditions()
        {
            clientAccess.Request("HardwareControl", "HardwareControl.PopInitialConditions");
        }

        /// <summary>
        /// Set the mode to the specified value.
        /// </summary>
        /// <param name="modeName">Mode name.</param>
        /// <param name="modeValue">Mode value.</param>
        public void SetMode(string modeName, string modeValue)
        {
            var newMode = new InstrumentMode
            {
                Name = modeName,
                Value = modeValue
            };
            clientAccess.Request("HardwareControl", "HardwareControl.SetMode", newMode);
        }

        /// <summary>
        /// Get the current value of the specified mode.
        /// </summary>
        /// <param name="modeName"></param>
        /// <returns></returns>
        public string GetMode(string modeName)
        {
            return modes.Get<CurrentMode>(modeName).ActiveValue;
        }

        /// <summary>
        /// Returns true if the specified instrument mode exists.
        /// </summary>
        /// <param name="modeName">Parameter/key name; for example "OpticMode".</param>
        /// <returns></returns>
        public bool ModeExists(string modeName)
        {
            return modes.Exists(modeName);
        }

        /// <summary>
        /// Get all the instrument modes with current state
        /// </summary>
        public IEnumerable<ModeValue> GetInstrumentModes()
        {
            foreach (var key in modes.GetKeys())
            {
                var modeValue = new ModeValue
                {
                    Name = key,
                    State = GetMode(key)
                };
                yield return modeValue;
            }
        }

        /// <summary>
        /// Get the definition of the specified instrument mode.
        /// </summary>
        /// <param name="modeName">Parameter/key name; for example "OpticMode".</param>
        /// <returns></returns>
        public InstrumentModeSet GetModeConfig(string modeName)
        {
            return modes.Get<CurrentMode>(modeName).Config;
        }

        /// <summary>
        /// Load Factory Settings.
        /// </summary>
        public void LoadFactorySetting()
        {
            clientAccess.Request("TuningApplication", "TuningApplication.LoadFactorySettings");
        }

        /// <summary>
        /// Save Factory Settings.
        /// </summary>
        public void SaveFactorySetting()
        {
            clientAccess.Request("TuningApplication", "TuningApplication.SaveFactorySettings");
        }

        /// <summary>
        /// Reset to Default Settings.
        /// </summary>
        public void ResetToDefaultSettings()
        {
            clientAccess.Request("TuningApplication", "TuningApplication.ResetToDefaultSettings");
        }

        /// <summary>
        /// Reset to Factory Settings.
        /// </summary>
        public void ResetToFactorySettings()
        {
            clientAccess.Request("TuningApplication", "TuningApplication.ResetNonFactoryToDefaultSettings");
        }

        /// <summary>
        /// Set the instrument into the operate state.
        /// </summary>
        public void Operate()
        {
            Set("Voltages.Operate.Setting", OperateSettings.Operate.ToString());
        }

        private void OnParametersRoomChange(string key)
        {
            if (Exists(key))
            {
                var changedValue = new ParameterValue
                {
                    Name = key,
                    Value = GetSetting(key)
                };
                ParameterChange(changedValue);
            }
        }

        private void OnModesRoomChange(string key)
        {
            if (ModeExists(key))
            {
                var changedValue = new ModeValue
                {
                    Name = key,
                    State = GetMode(key)
                };
                ModeChange(changedValue);
            }
        }

        //private void OnOnline(string online)
        //{
        //    IsOnline = String.Compare(online, "true", true) == 0;
        //    OnlineEvent(IsOnline);
        //}

        private void FireFactorySettingSavedComplete()
        {
            FactorySettingSavedComplete();
        }

        public void DisableDelayedSettling()
        {
            clientAccess.Request("HardwareControl", "Settling.DisableDelayedSettling");
        }

        public void Dispose()
        {
            instrumentParameter.KeyChanged -= OnParametersRoomChange;
            modes.KeyChanged -= OnModesRoomChange;

            // TODO Need to unregister - mainRoom.Subscribe<string>("Online", o => OnOnline(o));

            clientAccess.UnregisterHandler("HardwareControl.FactorySettingSaved", FireFactorySettingSavedComplete);

            instrumentParameter = null;
            modes = null;
            mainRoom = null;
        }
    }
}