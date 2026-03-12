using System.Globalization;

namespace AmbiMass.SpectrumStream.Contracts.Interfaces
{
    public interface INumberFormatProvider
    {
        NumberFormatInfo NumberFormat { get; }
    }
}
