using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbiMass.SpectrumStream.Data.RealtimeData
{
    public class ScanStarted
    {
        public int ScanId{ get; set; }
        public int Count { get; set; }

        public int TotalBytes{ get; set; }

        public int TotalChunks{ get; set; }

    }
}
