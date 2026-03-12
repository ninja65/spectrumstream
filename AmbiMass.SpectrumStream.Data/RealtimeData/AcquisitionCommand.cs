using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbiMass.SpectrumStream.Data.RealtimeData
{
    public class AcquisitionCommand
    {
        public AcquisitionCommand() { }

        public string? MSSettingsFile{ get;set;}

        public string? DateFile{ get; set; }
    }
}
