//
// Copyright © 2015 Waters Corporation. All Rights Reserved.
//


using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace Waters.Control.Client.InternalInterface
{
    /// <summary>
    /// Class storing callback actions associated with an xml element.
    /// </summary>
    [DebuggerDisplay("Name={Name}")]
    internal class ElementCallback
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementCallback"/> class.
        /// </summary>
        /// <param name="elementName"></param>
        /// <param name="start"></param>
        public ElementCallback(string elementName, Action<XElement> start) : this(elementName, start, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementCallback"/> class.
        /// </summary>
        /// <param name="elementName"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public ElementCallback(string elementName, Action<XElement> start, Action<XElement> end)
        {
            Name = elementName;
            Start = start;
            End = end;
        }

        /// <summary>
        /// Get the element name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Get the callback/action to perform when the element starts.
        /// </summary>
        public Action<XElement> Start { get; private set; }

        /// <summary>
        /// Get the callback/action to perform when the element ends.
        /// </summary>
        public Action<XElement> End { get; private set; }
    }
}
