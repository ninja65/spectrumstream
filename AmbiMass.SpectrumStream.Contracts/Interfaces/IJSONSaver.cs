namespace AmbiMass.SpectrumStream.Contracts.Interfaces
{
    public interface IJSONSaver
    {
        void saveToFile(string filename, object obj );
        string saveToString( object obj);
    }
}
