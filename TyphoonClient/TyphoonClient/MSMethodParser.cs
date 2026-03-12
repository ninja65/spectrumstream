////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2013-2017 Waters Corporation.
//
// MS method parser implementation to parse the method xml into MSMethod.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml.Linq;
using Waters.Control.Client.InternalInterface;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    /// <summary>
    /// MS Method parser class.
    /// </summary>
    public class MSMethodParser
    {
        private readonly MSMethodBuilderEngine builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSMethodParser"/> class.
        /// </summary>
        public MSMethodParser()
        {
            builder = new MSMethodBuilderEngine();

            // register rules on how to handle xml elements
            // do not need rules for container elements such as <Settings> or <TrackedParameters>
            builder.Register("Setup", x => OnSetup(x), x => OnEndSetup(x));
            builder.Register("PreSetup", x => OnPreSetup(x), x => OnEndPreSetup(x));
            builder.Register("PostSetup", x => OnPostSetup(x), x => OnEndPostSetup(x));
            builder.Register("Function", x => OnFunction(x));
            builder.Register("Mode", x => OnMode(x));
            builder.Register("Instance", x => OnInstance(x));
            builder.Register("Setting", x => OnSetting(x));
            builder.Register("Table", x => OnTable(x), x => OnEndTable(x));
            builder.Register("Row", x => OnRow(x), x => OnEndRow(x));

            builder.Register("DDA", x => OnDDA(x));
            builder.Register("TimedEvent", x => OnTimedEvent(x));
            builder.Register("Interrupt", x => OnInterrupt(x));
            builder.Register("TrackedParameter", x => OnTrackedParameter(x));
            builder.Register("Options", x => OnOptions(x));
            builder.Register("MethodScript", x => OnMethodScript(x));
            builder.Register("FunctionScript", x => OnFunctionScript(x));

            builder.Register("MetaData", x => OnMetaData(x));
            builder.Register("DataStorage", x => OnDataStorage(x));
            builder.Register("MethodSource", x => OnMethodSource(x));
        }

        /// <summary>
        /// Parse the supplied method xml to MS method
        /// </summary>
        /// <param name="methodXml">The method xml</param>
        /// <returns>The MS method</returns>
        public MSMethod Parse(string methodXml)
        {
            if (String.IsNullOrEmpty(methodXml))
            {
                throw new ArgumentNullException("Null or empty xml");
            }

            XElement element = XElement.Parse(methodXml, LoadOptions.None);
            return Parse(element);
        }

        /// <summary>
        /// Parse the supplied method xml element into MS method
        /// </summary>
        /// <param name="element">The method xml element</param>
        /// <returns>The MS method</returns>
        public MSMethod Parse(XElement element)
        {
            if (element.Name.LocalName != "MsMethod")
            {
                throw new ArgumentException("Wrong root in MS Method");
            }

            //builder.Evaluate(element);
            builder.Read(element);

            // build
            var typhoonMethod = builder.Build();
            typhoonMethod.Header = CreateHeader(element);

            return typhoonMethod;
        }


        private void OnSetup(XElement element)
        {
            // <Setup Type="..." Acquisition="..." />
            //   <Settings>
            //     <Setting Name="..." Value="..."/>
            //     ...
            //   </Settings>
            // </Setup>
            OnSetup(element, SetupType.PreSetup);
        }

        private void OnEndSetup(XElement element)
        {
            builder.MethodBuilder.EndSetup();
        }

        private void OnPreSetup(XElement element)
        {
            // <PreSetup Type="..." Acquisition="..." />
            //   <Settings>
            //     <Setting Name="..." Value="..."/>
            //     ...
            //   </Settings>
            // </PreSetup>
            var type = element.Attribute("Type").Value;

            var acquisition = String.Empty;
            if (element.HasAttribute("Acquisition"))
            {
                acquisition = element.Attribute("Acquisition").Value;
            }

            builder.MethodBuilder.PreSetup(type, acquisition);
        }

        private void OnEndPreSetup(XElement element)
        {
            builder.MethodBuilder.EndSetup();
        }

        private void OnPostSetup(XElement element)
        {
            // <PostSetup Type="..." Acquisition="..." />
            //   <Settings>
            //     <Setting Name="..." Value="..."/>
            //     ...
            //   </Settings>
            // </PostSetup>
            var type = element.Attribute("Type").Value;

            var acquisition = String.Empty;
            if (element.HasAttribute("Acquisition"))
            {
                acquisition = element.Attribute("Acquisition").Value;
            }

            builder.MethodBuilder.PostSetup(type, acquisition);
        }

        private void OnEndPostSetup(XElement element)
        {
            builder.MethodBuilder.EndSetup();
        }

        private void OnFunction(XElement element)
        {
            // <Function Type="..." TimeStart="..." TimeEnd="..."/>

            string type = (string)element.Attribute("Type");
            builder.MethodBuilder.Function(type);

            if (element.HasAttribute("TimeStart") && element.HasAttribute("TimeEnd"))
            {
                double startTime = element.GetAttributeValueAsDouble("TimeStart");
                double endTime = element.GetAttributeValueAsDouble("TimeEnd");
                builder.MethodBuilder.RetentionWindow(startTime, endTime);
            }

            // recurse for child elements
            //builder.Evaluate(element);
        }

        private void OnMode(XElement element)
        {
            // <Modes>
            //    <Mode Name="TOFMode" Value="IMS"/>
            // </Modes>

            string name = (string)element.Attribute("Name");
            string value = (string)element.Attribute("Value");

            builder.MethodBuilder.Mode(name, value);
        }

        private void OnInstance(XElement element)
        {
            //   <Instance>
            //     <Settings>
            //       <Setting Name="..." Value="..."/>
            //       ...
            //     </Settings>
            //  </Instance>
            builder.MethodBuilder.Instance();

            // recurse for child elements
            //builder.Evaluate(element);
        }

        private void OnSetting(XElement element)
        {
            // <Setting Name="..." Value="..." Mapping="..." MainOnly="..." /> or <Setting Name="..." Value="..."/>

            string name = (string)element.Attribute("Name");
            double value = element.GetAttributeValueAsDouble("Value");

            string mapping = String.Empty;
            if (element.HasAttribute("Mapping"))
            {
                mapping = (string)element.Attribute("Mapping");
            }

            bool mainOnly = false;
            mainOnly = element.GetAttributeValueAsBoolean("MainOnly");

            // add to list of settings in current context
            builder.MethodBuilder.Setting(name, value, mapping, mainOnly);
        }

        private void OnTable(XElement element)
        {
            // <Table Name="...">...</Table>

            string name = (string)element.Attribute("Name");
            builder.MethodBuilder.Table(name);
        }

        private void OnEndTable(XElement element)
        {
            builder.MethodBuilder.EndTable();
        }

        private void OnRow(XElement element)
        {
            builder.MethodBuilder.Row();
        }

        private void OnEndRow(XElement element)
        {
            builder.MethodBuilder.EndRow();
        }

        private void OnDDA(XElement element)
        {
            // <DDA Type="..." />

            string type = (string)element.Attribute("Type");
            builder.MethodBuilder.DDA(type);
        }

        private void OnTimedEvent(XElement element)
        {
            // <TimedEvent Time="..."> or <TimedEvent Interval="...">
            //     <Settings>
            //         <Setting Name="..." Value="..." Mapping="..." />
            //         ...
            //     </Settings>
            // </TimedEvent>

            builder.MethodBuilder.TimedEvent();

            if (element.HasAttribute("Time"))
            {
                double time = element.GetAttributeValueAsDouble("Time");
                builder.MethodBuilder.Time(time);
            }

            if (element.HasAttribute("Interval"))
            {
                double interval = element.GetAttributeValueAsDouble("Interval");
                builder.MethodBuilder.Interval(interval);
            }
        }

        private void OnInterrupt(XElement element)
        {
            //<TimedEvent Time="10">
            //    <Interrupt RepeatCount="10"> or <Interrupt RepeatTime="5">
            //        <Function Type="MS">
            //            <Instance>
            //                <Settings>
            //                    <Setting Name="StartMass" Value="100.0"/>
            //                    ...
            //                </Settings>
            //            </Instance>
            //        </Function>
            //    </Interrupt>
            //</TimedEvent>

            throw new InvalidOperationException("Interrupt not supported");
            //if (element.HasAttribute("RepeatCount"))
            //{
            //    int repeatCount = element.GetAttributeValueAsInt32("RepeatCount");
            //    builder.MethodBuilder.RepeatCount(repeatCount);
            //}

            //if (element.HasAttribute("RepeatTime"))
            //{
            //    int repeatTime = element.GetAttributeValueAsInt32("RepeatTime");
            //    builder.MethodBuilder.RepeatCount(repeatTime);
            //}
        }

        private void OnTrackedParameter(XElement element)
        {
            // <TrackedParameter Name="..."/>

            string name = (string)element.Attribute("Name");
            builder.MethodBuilder.TrackedParameter(name);
        }

        private void OnOptions(XElement element)
        {
            if (element.HasAttribute("RestoreState"))
            {
                bool restoreState = (bool)element.Attribute("RestoreState");
                builder.MethodBuilder.RestoreState(restoreState);
            }
        }

        private void OnMethodScript(XElement element)
        {
            // <FunctionScript Name="script name" />
            string name = (string)element.Attribute("Name");
            builder.MethodBuilder.MethodScript(name);
        }

        private void OnFunctionScript(XElement element)
        {
            // <FunctionScript Name="script name" />
            string name = (string)element.Attribute("Name");
            builder.MethodBuilder.FunctionScript(name);
        }

        private void OnMetaData(XElement element)
        {
            var name = (string)element.Attribute("Name");
            var value = (string)element.Attribute("Value");

            builder.MethodBuilder.MetaData(name, value);
        }

        private void OnDataStorage(XElement element)
        {
            var name = (string)element.Attribute("Filename");
            builder.MethodBuilder.DataStorage().Filename(name);

            if (element.HasAttribute("Type"))
            {
                var type = (string)element.Attribute("Type");
                builder.MethodBuilder.DataStorage().StorageType(type);
            }
        }

        private void OnMethodSource(XElement element)
        {
            // <MethodSource Source="..."/>

            string s = (string)element.Attribute("Source");
            builder.MethodBuilder.MethodSource(s);
        }

        private MethodHeader CreateHeader(XElement element)
        {
            var header = new MethodHeader
            {
                InstrumentType = (string) element.Attribute("InstrumentType"),
                InstrumentModel = (string) element.Attribute("InstrumentModel"),
                Version = (string) element.Attribute("Version")
            };

            return header;
        }

        private void OnSetup(XElement element, SetupType setupContext)
        {
            // <Setup Type="..." Acquisition="..." />
            //   <Settings>
            //     <Setting Name="..." Value="..."/>
            //     ...
            //   </Settings>
            // </Setup>

            var type = element.Attribute("Type").Value;

            var acquisition = String.Empty;
            if (element.HasAttribute("Acquisition"))
            {
                acquisition = element.Attribute("Acquisition").Value;
            }

            builder.MethodBuilder.Setup(type, acquisition);
        }

        private void OnPostSetup(XElement element, SetupType setupContext)
        {
            // <Setup Type="..." Acquisition="..." />
            //   <Settings>
            //     <Setting Name="..." Value="..."/>
            //     ...
            //   </Settings>
            // </Setup>

            var type = element.Attribute("Type").Value;

            var acquisition = String.Empty;
            if (element.HasAttribute("Acquisition"))
            {
                acquisition = element.Attribute("Acquisition").Value;
            }

            builder.MethodBuilder.PostSetup(type, acquisition);
        }
    }
}
