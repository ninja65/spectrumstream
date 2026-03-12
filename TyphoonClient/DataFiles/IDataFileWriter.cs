using System;
using Waters.Control.Message;

namespace Waters.Control.Client
{
    public interface IDataFileWriter : IDisposable
    {
        void Append(ScanData scanData);
    }
}