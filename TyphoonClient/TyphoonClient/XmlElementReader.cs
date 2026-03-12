//
// Copyright © 2015 Waters Corporation. All Rights Reserved.
//


using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Waters.Control.Client.InternalInterface
{
    /// <summary>
    /// Provides read-only firehose xml reader notifying of element start and element end.
    /// </summary>
    internal class XmlElementReader
    {
        /// <summary>
        /// Event fired at the start of an xml element.
        /// </summary>
        public event Action<string, IList<XAttribute>> StartElement = (e, a) => { };

        /// <summary>
        /// Event fired at the end of an xml element.
        /// </summary>
        public event Action<string> EndElement = (e) => { };

        /// <summary>
        /// Read the xml, fires <see cref="StartElement"/> and <see cref="EndElement"/> events for each element.
        /// </summary>
        /// <param name="element"></param>
        public void Read(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException();
            }

            using (var reader = element.CreateReader())
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // found the start of an element

                        // read a fragment, but not the entire element and its children/content
                        // (do not do var element = XElement.ReadFrom(reader); )
                        // as this will advance the reader and so skip the end element which we need to know.

                        // For example:
                        // <Function Type="..." TimeStart="..." TimeEnd="...">
                        //   <Instance>
                        //     ...
                        //   </Instance>
                        // </Function>

                        // we want just the <Function Type="..." TimeStart="..." TimeEnd="...">
                        // and not the entire  <Function ...>child elements </Function>

                        // This also avoids having to read the entire element and consequent recursive madness.

                        string elementName = reader.Name;

                        var attributes = GetAttributes(reader);

                        // fire event
                        StartElement(elementName, attributes);

                        // Self closing element doesn't fire End element so checking the IsEmptyElement
                        // and firing the EndElement ex- <root />
                        if(reader.IsEmptyElement)
                        {
                            // fire event
                            EndElement(elementName);
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        // found the end of an element
                        string elementName = reader.Name;

                        // fire event
                        EndElement(elementName);
                    }
                } // while
            }
        }

        /// <summary>
        /// Get the attributes of the current element; advances the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private List<XAttribute> GetAttributes(XmlReader reader)
        {
            var attributeList = new List<XAttribute>();

            if (reader.HasAttributes)
            {
                // enumerate all attributes
                while (reader.MoveToNextAttribute())
                {
                    var attribute = new XAttribute(reader.Name, reader.Value);
                    attributeList.Add(attribute);
                }

                // move back to the element that contains the attribute(s) we just traversed
                reader.MoveToElement();
            }

            return attributeList;
        }
    }
}
