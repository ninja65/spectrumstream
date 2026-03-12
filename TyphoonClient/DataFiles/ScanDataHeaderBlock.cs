namespace Waters.Control.Client
{
    public class ScanDataHeaderBlock
    {
        public byte[] Magic { get; set; } = { 0x53, 0x44 }; // => 'SD'
        public byte Version { get; set; } = 1;
        public byte Type { get; set; } = 1;
    }
}