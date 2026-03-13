using AmbiMass.SpectrumStream.Contracts.Interfaces;
using AmbiMass.SpectrumStream.Data.RealtimeData;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbiMass.SpectrumStream.Communication
{
    public class SignalRHubImpl : ISignalRHub
    {
        private readonly IHubContext<SpectrumStreamHub, ISignalRHub> _hub;

        public SignalRHubImpl(IHubContext<SpectrumStreamHub, ISignalRHub> hub)
        {
            _hub = hub;
        }

        public Task scanChunk(ScanChunk scanChunk)
        {
            return _hub.Clients.All.scanChunk( scanChunk );
        }

        public Task scanCompleted(ScanCompleted scanCompleted)
        {
            return _hub.Clients.All.scanCompleted( scanCompleted );
        }

        public Task scanStarted( ScanStarted scanStarted )
        {
            return _hub.Clients.All.scanStarted( scanStarted );
        }

        public Task scanFailed(ScanFailed scanFailed )
        {
            return _hub.Clients.All.scanFailed( scanFailed );
        }

    }
}
