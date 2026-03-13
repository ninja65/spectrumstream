using AmbiMass.SpectrumStream.Data.RealtimeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbiMass.SpectrumStream.Contracts.Interfaces
{
    public interface ISignalRHub
    {
        Task scanChunk(ScanChunk scanChunk);
        Task scanCompleted(ScanCompleted scanCompleted);
        Task scanStarted( ScanStarted scanStarted );
        
        Task scanFailed(ScanFailed scanFailed );
    }
}
