using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbiMass.SpectrumStream.Data.RealtimeData
{
    public class ScanStarted
    {
        public int scanId{ get; set; }
        public int count { get; set; }

        public int totalBytes{ get; set; }

        public int totalChunks{ get; set; }

        public int chunkSize{ get; set; }

    }
}
