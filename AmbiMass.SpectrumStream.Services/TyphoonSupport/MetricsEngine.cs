using System;
using Waters.Control.Client;

namespace AmbiMass.SpectrumStream.Services.TyphoonSupport
{
    public class MetricsEngine
    {
        public TyphoonMetrics Metrics { get; private set; }

        public MetricsEngine()
        {
            Metrics = new TyphoonMetrics
            {
                Time = DateTime.Now,
            };
        }

        public void refresh()
        {
            var hardwareControl = TyphoonFactory.Instance.HardwareControl;

            Metrics.Time = DateTime.Now;
            Metrics.InstrumentMetrics.SerialNumber = hardwareControl.GetInstrumentParameterValue("Instrument.SerialNumber").StringValue;
            Metrics.InstrumentMetrics.FluidicsRemaining = hardwareControl.GetInstrumentParameterValue("Fluidics.VolumeLeft.Readback").DoubleValue;
            Metrics.InstrumentMetrics.TofPressure = hardwareControl.GetInstrumentParameterValue("DCC.TOFPressure.Readback").DoubleValue;
            Metrics.InstrumentMetrics.SourcePressure = hardwareControl.GetInstrumentParameterValue("Source.SourcePressureDigital.Readback").DoubleValue;
            Metrics.InstrumentMetrics.CapillaryVoltage = hardwareControl.GetInstrumentParameterValue("Source.CapillaryVoltage.Setting").DoubleValue;
            Metrics.InstrumentMetrics.DetectorVoltage = hardwareControl.GetInstrumentParameterValue("Detector.MCPDetectorVoltage.Setting").DoubleValue;
        }
    }
}