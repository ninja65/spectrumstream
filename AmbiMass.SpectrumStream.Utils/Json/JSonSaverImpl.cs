
using AmbiMass.SpectrumStream.Contracts.Interfaces;

namespace AmbiMass.SpectrumStream.Utils.Json
{
    public class JSonSaverImpl : IJSONSaver
    {
        public JSonSaverImpl() { }

        public void saveToFile(string filename, object obj)
        {
            File.WriteAllText(filename, Newtonsoft.Json.JsonConvert.SerializeObject(obj));
        }

        public string saveToString( object obj) 
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
    }
}
