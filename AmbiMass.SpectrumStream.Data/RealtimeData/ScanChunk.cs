using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbiMass.SpectrumStream.Data.RealtimeData
{
    public class ScanChunk
    {
         public int scanId{ get; set; }
         public int chunkIndex{ get; set; }
         public int totalChunks{ get; set; }

         public int chunkLength { get; set; }

         public int itemsInChunk { get; set; }

         public byte[] data{ get; set; }
    }
}
