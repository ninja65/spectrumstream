//
// Copyright © 2015 Waters Corporation. All Rights Reserved.
//


using System;
using System.Xml.Linq;

namespace Waters.Control.Client.InternalInterface
{
    /// <summary>
    /// Extension methods for the XElement class.
    /// </summary>
    public static class XElementExtensions
    {
        /// <summary>
        /// Returns the value of the attribute as a double.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static double GetAttributeValueAsDouble(this XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute != null)
            {
                return (double)attribute;
            }
            else
            {
                return 0d;
            }
        }

        /// <summary>
        /// Returns the value of the attribute as a boolean.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static bool GetAttributeValueAsBoolean(this XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute != null)
            {
                return (bool)attribute;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the value of the attribute as an Int32.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static int GetAttributeValueAsInt32(this XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute != null)
            {
                return (Int32)attribute;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns true if the XElement has an attribute with the matching name.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static bool HasAttribute(this XElement element, string attributeName)
        {
            return element.Attribute(attributeName) != null;
        }
    }
}
