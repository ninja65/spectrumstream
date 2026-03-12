using AmbiMass.SpectrumStream.Contracts.Interfaces;

namespace AmbiMass.SpectrumStream.Utils.Json
{
    public class JSonLoaderImpl : IJSONLoader
    {
        public JSonLoaderImpl() { }

        public T loadFromFile<T>(string filename) where T : class
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
        }

        public T loadFromString<T>(string json) where T : class
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}
