using System;
using System.Collections.Generic;
using System.Globalization;
using Waters.Control.Message;
using System.Text;
using Google.Protobuf;

namespace Waters.Control.Client
{
    public enum SetupType { PreSetup, PostSetup }

    public class MSMethodBuilder
    {
        private MSMethod msMethod = new MSMethod();
        private MSSetup currentSetup;
        private MSFunction currentFunction;
        private MSInstance currentInstance;
        private DDAInstance currentDDAInstance;
        private TimedEvent currentTimedEvent;
        private readonly Stack<MethodTable> currentTableStack = new Stack<MethodTable>();

        public static MSMethodBuilder Create()
        {
            return new MSMethodBuilder();
        }

        public static MSMethodBuilder FromMethod(MSMethod method)
        {
            return new MSMethodBuilder { msMethod = method };
        }

        public MSMethod Build()
        {
            return msMethod;
        }

        public MSMethodBuilder Id(string acquisitionId)
        {
            msMethod.Id = new MSMethodId { AcquisitionId = acquisitionId };
            return this;
        }

        public MSMethodBuilder Mode(string name, int value)
        {
            var text = value.ToString(CultureInfo.InvariantCulture);
            return Mode(name, text);
        }

        public MSMethodBuilder Mode(string name, double value)
        {
            var text = value.ToString(CultureInfo.InvariantCulture);
            return Mode(name, text);
        }

        public MSMethodBuilder Mode(string name, string value)
        {
            var mode = new InstrumentMode { Name = name, Value = value };
            if (currentFunction != null)
                currentFunction.Mode.Add(mode);
            else
                msMethod.Mode.Add(mode);

            return this;
        }

        public MSMethodBuilder Setup(string type, string acquisitionType = null)
        {
            if (msMethod.Function.Count > 0)
            {
                return Setup(type, SetupType.PostSetup, acquisitionType);
            }
            return Setup(type, SetupType.PreSetup, acquisitionType);
        }

        public MSMethodBuilder PostSetup(string type, string acquisitionType = null)
        {
            return Setup(type, SetupType.PostSetup, acquisitionType);
        }
        public MSMethodBuilder PreSetup(string type, string acquisitionType = null)
        {
            return Setup(type, SetupType.PreSetup, acquisitionType);
        }

        public MSMethodBuilder EndSetup()
        {
            currentSetup = null;
            return this;
        }

        public MSMethodBuilder SetupWithOption(string type, SetupType setupContext, string option, string acquisitionType = null)
        {
            ResetCurrent();
            currentSetup = new MSSetup
            {
                Type = type,
                SetupContext = ConvertSetupType(setupContext),
                Option = option
            };

            if (!string.IsNullOrEmpty(acquisitionType))
            {
                currentSetup.AcquisitionType = acquisitionType;
            }

            if (setupContext == SetupType.PostSetup)
            {
                msMethod.PostSetup.Add(currentSetup);
            }
            else
            {
                msMethod.PreSetup.Add(currentSetup);
            }

            return this;
        }

        public MSMethodBuilder SetupWithOption(string type, string option, string acquisitionType = null)
        {
            return SetupWithOption(type, SetupType.PreSetup, option, acquisitionType);
        }

        public MSMethodBuilder Function(string type)
        {
            ResetCurrent();

            currentFunction = new MSFunction() { Type = type };

            if (currentTimedEvent != null)
                currentTimedEvent.Interrupt.Function.Add(currentFunction);
            else
                msMethod.Function.Add(currentFunction);

            return this;
        }

        public MSMethodBuilder Instance()
        {
            ResetTableStack();

            currentInstance = new MSInstance();
            currentFunction.Instance.Add(currentInstance);

            currentDDAInstance = null;

            return this;
        }

        /// <summary>
        /// Add a new setting to the current DDA, instance, function, or method.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="value">Setting value.</param>
        /// <param name="mapping">Optional mapping.</param>
        /// <param name="mainOnly">Applied to main method only</param>
        /// <returns></returns>
        public MSMethodBuilder Setting(string name, double value, string mapping = "", bool mainOnly = false)
        {
            var msgValue = new VariantValue() { DoubleValue = value };
            var setting = new MethodSetting() { Name = name, Value = msgValue, Mainonly = mainOnly };

            var luaName = GetMapping(name, mapping);
            if (luaName != string.Empty)
            {
                setting.Mapping = luaName;
            }

            GetCurrentTable().Setting.Add(setting);

            return this;
        }

        public MSMethodBuilder Setting(string name, string value, string mapping = "", bool mainOnly = false)
        {
            var msgValue = new VariantValue() { StringValue = value };
            var setting = new MethodSetting() { Name = name, Value = msgValue, Mainonly = mainOnly };

            var luaName = GetMapping(name, mapping);
            if (luaName != string.Empty)
            {
                setting.Mapping = luaName;
            }

            GetCurrentTable().Setting.Add(setting);

            return this;
        }

        //public MSMethodBuilder Setting(string name, byte[] value, string mapping = "", bool mainOnly = false)
        //{
        //    var msgValue = new VariantValue() { EncodedValue = value };
        //    var setting = new MethodSetting() { Name = name, Value = msgValue, Mainonly = mainOnly };

        //    var luaName = GetMapping(name, mapping);
        //    if (luaName != string.Empty)
        //    {
        //        setting.Mapping = luaName;
        //    }

        //    GetCurrentTable().Setting.Add(setting);

        //    return this;
        //}

        private string GetMapping(string name, string mapping = "")
        {
            if (!String.IsNullOrWhiteSpace(mapping))
            {
                return mapping;
            }
            else if (name.Contains('.'))
            {
                // if name is a hierarchical name assume its the mapping
                return name;
            }

            return string.Empty;
        }

        public MSMethodBuilder DDA(string ddaType, DDABranching branching = DDABranching.Interrupt)//default is  DDABranching.Interrupt used by Raven
        {
            ResetTableStack();

            currentDDAInstance = new DDAInstance() { Type = ddaType, InterruptBranching = branching };

            currentInstance.Dda.Add(currentDDAInstance);

            return this;
        }

        public MSMethodBuilder RetentionWindow(double startTime, double endTime)
        {
            currentFunction.RetentionWindow = new RetentionWindow { StartTime = startTime, EndTime = endTime };
            return this;
        }

        /// <summary>
        /// Add metadata key-value pair to the current instance, function, or method
        /// to store data-system specific context information.
        /// </summary>
        /// <remarks>
        /// Metadata key-value pairs are opaque to Typhoon and will be returned merged in with the data for each scan.
        /// </remarks>
        /// <param name="keyName">Key name.</param>
        /// <param name="value">Value.</param>
        /// <returns></returns>
        public MSMethodBuilder MetaData(string keyName, string value)
        {
            var valueAsBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(value);

            if (currentInstance != null)
            {
                currentInstance.Metadata = UpdateMetaData(currentInstance.Metadata, keyName, valueAsBytes);
            }
            else if (currentFunction != null)
            {
                currentFunction.Metadata = UpdateMetaData(currentFunction.Metadata, keyName, valueAsBytes);
            }
            else
            {
                msMethod.Metadata = UpdateMetaData(msMethod.Metadata, keyName, valueAsBytes);
            }

            return this;
        }

        public MSMethodBuilder DataStorage()
        {
            msMethod.DataStorage ??= new AcquisitionDataStorage();
            return this;
        }

        public MSMethodBuilder Filename(string filename)
        {
            msMethod.DataStorage.Filename = filename;
            return this;
        }

        public MSMethodBuilder StorageType(string storageType)
        {
            msMethod.DataStorage.Type = storageType;
            return this;
        }

        /// <summary>
        /// Add a new table to the current DDA, instance, or method.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        /// <remarks>
        /// Rows are implemented as sub-tables, and columns are implemented as settings.
        /// </remarks>
        public MSMethodBuilder Table(string tableName)
        {
            AppendNewTable(tableName);

            return this;
        }

        public MSMethodBuilder EndTable()
        {
            currentTableStack.Pop();
            return this;
        }

        /// <summary>
        /// Add a row to the active table.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Rows are implemented as sub-tables, and columns are implemented as settings.
        /// <para>
        /// Rows have a (sub-)table name with a one based index.
        /// </para>
        /// </remarks>
        public MSMethodBuilder Row()
        {
            AppendNewTable(string.Empty);

            return this;
        }

        public MSMethodBuilder EndRow()
        {
            // rows are stored as sub-tables, so end the current sub-table
            return EndTable();
        }

        /// <summary>
        /// Add a column to the active row in the active table.
        /// </summary>
        /// <param name="name">Column name.</param>
        /// <param name="value">Column value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Rows are implemented as sub-tables, and columns are implemented as settings.
        /// </remarks>
        /// <seealso cref="Setting"/>
        public MSMethodBuilder Column(string name, double value)
        {
            Setting(name, value, String.Empty);
            return this;
        }

        public MSMethodBuilder TimedEvent()
        {
            ResetCurrent();
            currentTimedEvent = new TimedEvent();
            currentTimedEvent.Timing ??= new TimedEventTiming();
            msMethod.TimedEvent.Add(currentTimedEvent);
            return this;
        }

        public MSMethodBuilder EndTimedEvent()
        {
            ResetCurrent();
            currentTimedEvent = null;
            return this;
        }

        public MSMethodBuilder Time(double time)
        {
            currentTimedEvent.Timing.Time = time;
            return this;
        }

        public MSMethodBuilder Interval(double interval)
        {
            currentTimedEvent.Timing.Interval = interval;
            return this;
        }

        public MSMethodBuilder Interrupt()
        {
            currentTimedEvent.Interrupt = new MSInterrupt();
            return this;
        }

        public MSMethodBuilder TrackedParameter(string name)
        {
            msMethod.TrackedParameter.Add(name);
            return this;
        }

        public MSMethodBuilder RestoreState(bool restore)
        {
            msMethod.Options.RestoreState = restore;
            return this;
        }

        public MSMethodBuilder MethodScript(string name)
        {
            msMethod.MethodScriptName.Add(name);
            return this;
        }

        public MSMethodBuilder FunctionScript(string name)
        {
            currentInstance.FunctionScriptName.Add(name);
            return this;
        }

        public MSMethodBuilder ResetToMethod()
        {
            ResetCurrent();
            return this;
        }

        public MSMethodBuilder MethodSource(string source)
        {
            msMethod.MethodSource = string.IsNullOrEmpty(source)
                        ? MethodSources.None
                        : (MethodSources)Enum.Parse(typeof(MethodSources), source);
            return this;
        }

        public MSMethodBuilder ResetToFunction(int function)
        {
            ResetCurrent();
            currentFunction = msMethod.Function[function];
            return this;
        }

        public MSMethodBuilder ResetToInstance(int function, int instance)
        {
            ResetCurrent();
            currentFunction = msMethod.Function[function];
            currentInstance = currentFunction.Instance[instance];
            return this;
        }

        private void ResetCurrent()
        {
            currentSetup = null;
            currentFunction = null;
            currentInstance = null;
            currentDDAInstance = null;

            ResetTableStack();
        }

        private void ResetTableStack()
        {
            currentTableStack.Clear();
        }

        /// <summary>
        /// Creates a new table and makes it the current table.
        /// </summary>
        /// <param name="tableName"></param>
        private void AppendNewTable(string tableName)
        {
            MethodTable newTable = new MethodTable();
            if (!string.IsNullOrEmpty(tableName))
                newTable.Name = tableName;

            GetCurrentTable().Table.Add(newTable);
            currentTableStack.Push(newTable);
        }

        /// <summary>
        /// Get the table for the current DDA/instance/setup/event/method.
        /// </summary>
        /// <returns></returns>
        private MethodTable GetCurrentTable()
        {
            MethodTable currentTable;

            if (currentTableStack.Count != 0)
            {
                currentTable = currentTableStack.Peek();
            }
            else if (currentDDAInstance != null)
            {
                currentDDAInstance.Settings ??= new MethodTable();
                currentTable = currentDDAInstance.Settings;
            }
            else if (currentInstance != null)
            {
                currentInstance.Settings ??= new MethodTable();
                currentTable = currentInstance.Settings;
            }
            else if (currentSetup != null)
            {
                currentSetup.Settings ??= new MethodTable();
                currentTable = currentSetup.Settings;
            }
            else if (currentTimedEvent != null)
            {
                currentTimedEvent.Settings ??= new MethodTable();
                currentTable = currentTimedEvent.Settings;
            }
            else
            {
                msMethod.InitialCondition ??= new MethodTable();
                currentTable = msMethod.InitialCondition;
            }

            return currentTable;
        }

        private KeyValueList UpdateMetaData(KeyValueList metadata, string keyName, byte[] valueAsBytes)
        {
            metadata ??= new KeyValueList();
            metadata.Items.Remove(new KeyValue { Key = keyName });
            metadata.Items.Add(new KeyValue { Key = keyName, Value = ByteString.CopyFrom(valueAsBytes) });
            return metadata;
        }

        private Waters.Control.Message.SetupType ConvertSetupType(SetupType type)
        {
            // convert SetupType enum in Waters.Control.Client namespace to Waters.Control.Message namespace
            return (type == SetupType.PostSetup ? Waters.Control.Message.SetupType.PostAcquisition
                                                : Waters.Control.Message.SetupType.PreAcquisition);
        }

        private MSMethodBuilder Setup(string type, SetupType setupContext, string acquisitionType = null)
        {
            ResetCurrent();
            currentSetup = new MSSetup
            {
                Type = type,
                SetupContext = ConvertSetupType(setupContext)
            };

            if (!string.IsNullOrEmpty(acquisitionType))
            {
                currentSetup.AcquisitionType = acquisitionType;
            }

            if (setupContext == SetupType.PostSetup)
            {
                msMethod.PostSetup.Add(currentSetup);
            }
            else
            {
                msMethod.PreSetup.Add(currentSetup);
            }

            return this;
        }
    }
}
