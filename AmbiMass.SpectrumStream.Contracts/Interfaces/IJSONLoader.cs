namespace AmbiMass.SpectrumStream.Contracts.Interfaces
{
    public interface IJSONLoader
    {
        T loadFromFile<T>(string filename) where T : class;
        T loadFromString<T>(string json) where T : class;
    }
}
