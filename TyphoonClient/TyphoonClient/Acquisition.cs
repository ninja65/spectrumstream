using System;
using System.Threading;
using Waters.Control.Client;
using Waters.Control.Client.Interface;
using Waters.Control.Message;

namespace TyphoonClient.TyphoonClient
{
    public class Acquisition
    {
        private readonly MSMethod method;
        private Result result = Result.OK;
        private readonly AutoResetEvent waitForComplete = new AutoResetEvent(false);

        public enum Result
        {
            OK,
            Error,
            Aborted
        }
        public event Action<Result> Complete;
        public event Action<Result> Aborted;
        public event Action<ScanData> ScanDataEvent;

        public Acquisition(MSMethod method)
        {
            this.method = method;
            method.Id = new MSMethodId
            {
                AcquisitionId = Guid.NewGuid().ToString("N")
            };

            TyphoonFactory.Instance.MethodRunner.ScanDataEvent += OnScanData;
            TyphoonFactory.Instance.MethodRunner.MethodEvent += OnMethodEvent;
        }

        public void Run()
        {
            TyphoonFactory.Instance.MethodRunner.Run(method);
        }

        public void Abort()
        {
            TyphoonFactory.Instance.MethodRunner.Abort();
        }

        public void WaitForCompletion()
        {
            waitForComplete.WaitOne();
        }

        private void OnScanData(ScanData scanData)
        {
            if (scanData.MethodId.AcquisitionId == method.Id.AcquisitionId)
            {
                ScanDataEvent?.Invoke(scanData);
            }
        }

        private void OnMethodEvent(MethodEvent me, MSMethodDetails details)
        {
            if (details.Id.AcquisitionId == method.Id.AcquisitionId)
            {
                switch (me)
                {
                    case MethodEvent.Complete:
                        Complete?.Invoke(result);
                        waitForComplete.Set();
                        break;
                    case MethodEvent.Aborted:
                        result = Result.Aborted;
                        Aborted?.Invoke(result);
                        break;
                    case MethodEvent.Error:
                        result = Result.Error;
                        break;
                }
            }
        }
    }
}
