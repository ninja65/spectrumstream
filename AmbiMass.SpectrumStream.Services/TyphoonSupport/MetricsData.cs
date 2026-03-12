using System;
using System.Collections.Generic;

namespace AmbiMass.SpectrumStream.Services.TyphoonSupport
{
    public class TyphoonMetrics
    {
        public DateTime Time { get; set; }
        public InstrumentMetrics InstrumentMetrics { get; set; } = new InstrumentMetrics();
    }

    public class InstrumentMetrics
    {
        public string SerialNumber { get; set; }
        public double FluidicsRemaining { get; set; }
        public double TofPressure { get; set; }
        public double SourcePressure { get; set; }
        public double CapillaryVoltage { get; set; }
        public double DetectorVoltage { get; set; }
    }

}