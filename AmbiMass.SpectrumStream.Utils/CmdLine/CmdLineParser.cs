using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbiMass.SpectrumStream.Utils.CmdLine
{
    public class CmdLineParser
    {
        private string[] _args;
        public CmdLineParser(string[] args) {
            _args = args;
        }

        public string stringValue( string key, string defaultValue  )
        {
            string keyToFind = $"--{key}=";

            string str = _args.FirstOrDefault( x => x.StartsWith(keyToFind) );

            if( string.IsNullOrWhiteSpace( str ) ) { 
                return defaultValue;
            }

            string value = str.Substring(keyToFind.Length );

            return value;
        }

        public int intValue( string key, int defaultValue )
        {
            var str = stringValue(key, defaultValue.ToString());

            if( !string.IsNullOrWhiteSpace( str ) )
            {
                return int.Parse( str );
            }

            return defaultValue;    
        }

        public bool boolValue(string key, bool defaultValue)
        {
            var str = stringValue(key, defaultValue.ToString());

            if (!string.IsNullOrWhiteSpace(str))
            {
                return str.ToLower() switch
                {
                    "true" => true,
                    "yes" => true,
                    _ => false
                };
            }

            return defaultValue;
        }

        public bool switchPresents(string key)
        {
            string keyToFind = $"--{key}";

            return _args.Contains(keyToFind);
        }
    }

}
