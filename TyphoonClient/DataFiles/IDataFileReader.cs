using System;
using System.Collections.Generic;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    public interface IDataFileReader : IDisposable
    {
        IEnumerable<ScanData> ReadScans();
    }
}