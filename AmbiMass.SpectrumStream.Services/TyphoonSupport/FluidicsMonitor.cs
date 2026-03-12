using Waters.Control.Client;
using Waters.Control.Client.Interface;

namespace AmbiMass.SpectrumStream.Services.TyphoonSupport
{
    public class FluidicsMonitor
    {
        private FluidicsSettings settings;
        private double currentIdleFlowRate;
        private string fluidicsStatus;
        private bool running;

        private bool Enabled => settings.AutoFluidics;

        public FluidicsMonitor(FluidicsSettings settings)
        {
            this.settings = settings;
            TyphoonFactory.Instance.HardwareControl.ParameterChange += OnParameterChange;

            if (Enabled && TyphoonFactory.Instance.HardwareControl.Exists("Fluidics.VolumeLeft.Readback"))
            {
                if (GetVolumeLeft() < settings.MinimumFluidicsVolume)
                {
                    Refill();
                }
                else
                {
                    RunAtIdleFlow();
                }
            }
        }

        public void StartRun(FluidicsSettings s)
        {
            settings = s;
            running = true;
            SetIdleFlowRate(settings.IdleFlowRate);
        }

        public void CompleteRun()
        {
            running = false;
            if (settings.IdleFlowRate <= 0.0)
            {
                Stop();
            }
        }

        private void OnParameterChange(ParameterValue value)
        {
            if (!Enabled)
            {
                return;
            }

            if (value.Name == "Fluidics.VolumeLeft.Readback")
            {
                OnVolumeLeft(value.Value.DoubleValue);
            }
            else if (value.Name == "Fluidics.OverallStatus.Readback")
            {
                OnStatusUpdate(value.Value.StringValue);
            }
        }

        private void OnVolumeLeft(double volumeLeft)
        {
            if (volumeLeft < settings.MinimumFluidicsVolume && OkToRefill())
            {
                Refill();
            }
        }

        private void OnStatusUpdate(string status)
        {
            fluidicsStatus = status;
            if (status == "IdleState")
            {
                RunAtIdleFlow();
            }
        }

        private void RunAtIdleFlow()
        {
            if (settings.IdleFlowRate > 0)
            {
                SetIdleFlowRate(settings.IdleFlowRate);
                StartFlow();
            }
        }

        private void SetIdleFlowRate(double flowRate)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (currentIdleFlowRate != flowRate)
            {
                TyphoonFactory.Instance.HardwareControl.Set("SampleFluidics.FlowRate.Setting", flowRate);
                currentIdleFlowRate = flowRate;
            }
        }

        private bool OkToRefill()
        {
            return !running && fluidicsStatus != "RefillingState";
        }

        private static void StartFlow()
        {
            TyphoonFactory.Instance.HardwareControl.Set("SampleFluidics.Start.Setting", "StartPump");
        }

        private static void Stop()
        {
            TyphoonFactory.Instance.HardwareControl.Set("SampleFluidics.Stop.Setting", "StopPump");
        }

        private void Refill()
        {
            Stop();
            TyphoonFactory.Instance.HardwareControl.Set("SampleFluidics.Refill.Setting", "Refill");

            // Immediately update status to refilling so if we get time left readback again before the refill status kicks 
            // we don't issue 2 refill commands
            fluidicsStatus = "RefillingState";
        }

        private static double GetVolumeLeft()
        {
            return TyphoonFactory.Instance.HardwareControl.GetSetting("Fluidics.VolumeLeft.Readback").DoubleValue;
        }
    }
}