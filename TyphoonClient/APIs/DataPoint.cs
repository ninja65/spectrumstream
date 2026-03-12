using System.Diagnostics;

namespace Waters.Control.Client
{
    [DebuggerDisplay("({Mass},{Intensity})")]
    public struct DataPoint
    {
        public double Mass;
        public double Intensity;
        public double PpmError;
    }
}