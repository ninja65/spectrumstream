namespace AmbiMass.SpectrumStream.Services.TyphoonSupport
{
    public class FluidicsSettings
    {
        public bool AutoFluidics { get; set; } = true;
        public double IdleFlowRate { get; set; } = 5.0;
        public double MinimumFluidicsVolume { get; set; } = 20;    // Need 30 seconds at acquisition flow rate, plus a bit more.
    }

    public class MSSettings
    {
        public FluidicsSettings Fluidics { get; set; } = new FluidicsSettings();
        public AcquisitionSettings Acquisition { get; set; } = new AcquisitionSettings();
    }

    public class AcquisitionSettings
    {
        public string Type { get; set; } = "MS";
        public string Polarity { get; set; } = "Negative";
        public double StartMass { get; set; } = 50.0;
        public double EndMass { get; set; } = 1100.0;
        public double ScanTime { get; set; } = 1.0;
        public double RunTime { get; set; } = 3000000.0;
        public double FlowRate { get; set; } = 30.0;
    }

}
