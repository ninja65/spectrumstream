using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbiMass.SpectrumStream.Data.RealtimeData
{
    public class ScanChunk
    {
         public int ScanId{ get; set; }
         public int ChunkIndex{ get; set; }
         public int TotalChunks{ get; set; }

         public int ChunkLength { get; set; }

         public int ItemsInChunk { get; set; }

         public byte[] Data{ get; set; }
    }
}
