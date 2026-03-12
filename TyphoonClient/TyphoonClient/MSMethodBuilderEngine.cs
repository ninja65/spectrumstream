//
// Copyright © 2015 Waters Corporation. All Rights Reserved.
//


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using Waters.Control.Message;

namespace Waters.Control.Client.InternalInterface
{
    /// <summary>
    /// Associates xml element names with callbacks to build aspects of a Typhoon method.
    /// </summary>
    [DebuggerDisplay("Count={Count}")]
    public class MSMethodBuilderEngine
    {
        private MSMethodBuilder builder = new MSMethodBuilder();

        private Dictionary<string, ElementCallback> map = new Dictionary<string, ElementCallback>();

        /// <summary>
        /// Register the rule.
        /// </summary>
        /// <param name="elementName">Xml element name.</param>
        /// <param name="startAction">Action or lambda expression accepting an xml element performed on element start.</param>
        public void Register(string elementName, Action<XElement> startAction)
        {
            Register(elementName, startAction, null);
        }

        /// <summary>
        /// Register the rule.
        /// </summary>
        /// <param name="elementName">Xml element name.</param>
        /// <param name="startAction">Action or lambda expression accepting an xml element performed on element start.</param>
        /// <param name="endAction">Optional action or lambda expression accepting an xml element performed on element end.</param>
        public void Register(string elementName, Action<XElement> startAction, Action<XElement> endAction)
        {
            if (String.IsNullOrEmpty(elementName))
            {
                throw new ArgumentNullException("elementName");
            }
            if (startAction == null)
            {
                throw new ArgumentNullException("startAction");
            }

            // endAction is optional

            map[elementName] = new ElementCallback(elementName, startAction, endAction);
        }

        /// <summary>
        /// Read the xml, will make callbacks according to the registered rules.
        /// </summary>
        /// <param name="xml"></param>
        public void Read(string xml)
        {
            var element = XElement.Parse(xml);

            Read(element);
        }

        /// <summary>
        /// Read the xml, will make callbacks according to the registered rules.
        /// </summary>
        /// <param name="element"></param>
        public void Read(XElement element)
        {
            var reader = new XmlElementReader();
            reader.StartElement += OnStartElement;
            reader.EndElement += OnEndElement;

            // read the xml and fire events
            reader.Read(element);
        }

        /// <summary>
        /// Build the Typhoon method.
        /// </summary>
        /// <returns></returns>
        public MSMethod Build()
        {
            return builder.Build();
        }

        /// <summary>
        /// Get the Typhoon method builder object.
        /// </summary>
        public MSMethodBuilder MethodBuilder
        {
            get { return builder; }
        }

        /// <summary>
        /// Get the number of rules (for unit testing).
        /// </summary>
        internal int Count
        {
            get { return map.Count; }
        }

        private void OnStartElement(string elementName, IList<XAttribute> attributeList)
        {
            Console.WriteLine("Found start element for {0} ({1} attribute(s))", elementName, attributeList.Count);

            // run the start action for this element
            if (map.ContainsKey(elementName))
            {
                var elementAction = map[elementName];

                if (elementAction.Start != null)
                {
                    // TODO -  consider change callback to be string elementName, IList<XAttribute> attributeList, rather than XElement?

                    // marshall parameters into an XElement
                    var fragment = new XElement(elementName);
                    foreach (var attribute in attributeList)
                    {
                        fragment.Add(attribute);
                    }

                    elementAction.Start(fragment);
                }
            }
        }

        private void OnEndElement(string elementName)
        {
            Console.WriteLine("Found end element for {0}", elementName);

            // run the end action for this element
            if (map.ContainsKey(elementName))
            {
                var elementAction = map[elementName];

                if (elementAction.End != null)
                {
                    // marshall parameters into an XElement
                    var fragment = new XElement(elementName);

                    elementAction.End(fragment);
                }
            }
        }
    }
}
