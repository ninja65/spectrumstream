using AmbiMass.SpectrumStream.Contracts.Interfaces;
using System.Globalization;

namespace AmbiMass.SpectrumStream.Utils.SysEnv
{
    public class NumberformatProviderImpl : INumberFormatProvider
    {
        private NumberFormatInfo _NumberFormat;
        public NumberformatProviderImpl()
        {
            _NumberFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();

            _NumberFormat.NumberDecimalSeparator = ".";
        }

        public NumberFormatInfo NumberFormat
        {
            get
            {
                return _NumberFormat;
            }
        }
    }
}
