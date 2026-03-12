using AmbiMass.SpectrumStream.Contracts.Interfaces;
using AmbiMass.SpectrumStream.Data.RealtimeData;
using AmbiMass.SpectrumStream.Services.Services;
using Microsoft.AspNetCore.SignalR;

namespace AmbiMass.SpectrumStream.Communication
{
    public class SpectrumStreamHub : Hub<ISignalRHub>
    {
        private readonly ILogger<SpectrumStreamHub> _logger;

        private readonly AcquisitionStarterImpl _acquisitionStarter;

        public SpectrumStreamHub(ILogger<SpectrumStreamHub> logger, 
            AcquisitionStarterImpl acquisitionStarter)
        {
            _logger = logger;

            _acquisitionStarter = acquisitionStarter;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Ping(string message)
        {
            _logger.LogInformation("Ping: {message} {ConnectionId}", message, Context.ConnectionId);
        }

        public async Task startAcquisition( AcquisitionCommand acquisitionCommand)
        {
            _acquisitionStarter.startAcquisition( acquisitionCommand);
        }

        public async Task stopAcquisition()
        {
            _acquisitionStarter.stopAcquisition();
        }
    }

}
