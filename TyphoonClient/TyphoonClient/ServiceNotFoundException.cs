////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2013 Waters Corporation.
//
// Typhoon service not found exception.
//
////////////////////////////////////////////////////////////////////////////

using System;

namespace Waters.Control.Client.InternalInterface
{
    /// <summary>
    /// Service not found exception
    /// </summary>
    public class ServiceNotFoundException : Exception
    {
        /// <summary>
        /// Service name
        /// </summary>
        public string Service { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="service">The service name</param>
        public ServiceNotFoundException(string service)
        {
            Service = service;
        }
    }
}
