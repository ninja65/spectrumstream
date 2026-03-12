using Autofac.Core;
using System;
using TyphoonClient.TyphoonClient;
using Waters.Control.Client;
using Waters.Control.Client.Interface;
using Waters.Control.Message;

namespace AmbiMass.SpectrumStream.Services.TyphoonSupport
{
    public class TyphoonSystem : IDisposable
    {
        private SystemStatus systemStatus;

        private Acquisition _Acquisition;

        public event Action<SystemStatus> SystemStatus;

        public event Action<Acquisition.Result> Complete;

        public event Action<ScanData> ScanDataEvent;

        private bool _disposed;


        public TyphoonSystem(string typhoonEndpoint)
        {
            TyphoonFactory.Create(new TyphoonClientConfiguration
            {
                EndPointUri = typhoonEndpoint
            });

            TyphoonFactory.Instance.SystemManager.ConnectionStatusChanged += OnSystemManagerStatus;
            TyphoonFactory.Instance.SystemMonitor.Status += OnSystemMonitorStatus;
            TyphoonFactory.Instance.SystemManager.Start();
            TyphoonFactory.Instance.SystemManager.PublishConnectionStatus();
        }

        public TyphoonSystem(TyphoonClientConfiguration typhoonClientConfig)
        {
            TyphoonFactory.Create(typhoonClientConfig);

            TyphoonFactory.Instance.SystemManager.ConnectionStatusChanged += OnSystemManagerStatus;
            TyphoonFactory.Instance.SystemMonitor.Status += OnSystemMonitorStatus;
            TyphoonFactory.Instance.SystemManager.Start();
            TyphoonFactory.Instance.SystemManager.PublishConnectionStatus();
        }

        public void startAcquisition(MSSettings settings)
        {
            var builder = new MSMethodBuilder();
            if (settings.Fluidics.AutoFluidics && settings.Acquisition.FlowRate > 0.0)
            {
                builder
                   .Setting("FlowRate", settings.Acquisition.FlowRate, "SampleFluidics.FlowRate.Setting")
                   .Setting("StartFluidics", "StartPump", "SampleFluidics.Start.Setting");
            }

            var method = builder
                        .Mode("Polarity", settings.Acquisition.Polarity)
                        .TimedEvent()
                        .Time(0.0)
                        .Setting("TriggerSwab", "Enable", "Inlet.Event2Out.Setting")
                        .TimedEvent()
                        .Time(1.0)
                        .Setting("ResetTrigger", "Disable", "Inlet.Event2Out.Setting")
                        .EndTimedEvent()
                        .Function(settings.Acquisition.Type)
                        .RetentionWindow(0, settings.Acquisition.RunTime)
                        .Instance()
                        .Setting("StartMass", settings.Acquisition.StartMass)
                        .Setting("EndMass", settings.Acquisition.EndMass)
                        .Setting("ScanTime", settings.Acquisition.ScanTime)
                        .Build();

            startAcquisition(method);
        }

        public void startAcquisition(MSMethod method)
        {
            _Acquisition = new Acquisition(method);
            _Acquisition.ScanDataEvent += scanData => ScanDataEvent?.Invoke(scanData);
            _Acquisition.Complete += result => Complete?.Invoke(result);
            _Acquisition.Run();
        }

        public void startAcquisition(string msMethodXml)
        {
            if (msMethodXml == null)
            {
                return;
            }
            var methodParser = new MSMethodParser();

            var method = methodParser?.Parse(msMethodXml);

            startAcquisition(method);
        }

        public void stopAcquisition()
        {
            _Acquisition?.Abort();
        }
        public SystemStatus GetSystemStatus()
        {
            return systemStatus;
        }

        private void OnSystemManagerStatus(object sender, TyphoonConnectionEventArg e)
        {
            systemStatus.OnlineStatus = e.ConnectionStatus == TyphoonConnectionStatus.SUCCEEDED
                ? OnlineStatus.Online
                : OnlineStatus.Offline;
            SystemStatus?.Invoke(systemStatus);
        }

        private void OnSystemMonitorStatus(Waters.Control.Client.Interface.SystemStatus status)
        {
            systemStatus.OnlineStatus = status == Waters.Control.Client.Interface.SystemStatus.NoResponse
                ? OnlineStatus.Offline
                : OnlineStatus.Online;
            SystemStatus?.Invoke(systemStatus);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose( bool disposing )
        {
            if (_disposed )
            {
                return;
            }
            if ( disposing ) 
            {
                TyphoonFactory.Instance.Destroy();
            }

            _disposed = true; 
        }

        public bool IsOnline
        {
            get
            {
                return systemStatus.OnlineStatus == OnlineStatus.Online;
            }
        }

    }
}