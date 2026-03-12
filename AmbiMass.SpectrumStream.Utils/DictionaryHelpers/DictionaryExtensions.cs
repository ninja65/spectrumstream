using Newtonsoft.Json;

namespace AmbiMass.SpectrumStream.Utils.DictionaryHelpers
{
    public static class DictionaryExtensions
    {
        public static T asObject<T>(this IDictionary<string, string> dictionary, string key) where T : class
        {
            if (dictionary.TryGetValue(key, out string value))
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            else
            {
                return default;
            }
        }

        public static string? asString(this IDictionary<string, string> dictionary, string key, string? defaultValue)
        {
            string? result = defaultValue;

            if (dictionary.TryGetValue(key, out string value))
            {
                result = value;
            }

            return result;
        }

        public static long? asLong(this IDictionary<string, string> dictionary, string key, long? defaultValue)
        {
            long? result = defaultValue;

            if (dictionary.TryGetValue(key, out string value))
            {
                long resultValue;

                if (!long.TryParse(value, out resultValue))
                {
                    result = defaultValue;
                }
                else
                {
                    result = resultValue;
                }
            }

            return result;
        }

        public static int? asInt(this IDictionary<string, string> dictionary, string key, int? defaultValue)
        {
            int? result = defaultValue;

            if (dictionary.TryGetValue(key, out string value))
            {
                int resultValue;

                if (!int.TryParse(value, out resultValue))
                {
                    result = defaultValue;
                }
                else
                {
                    result = resultValue;
                }
            }

            return result;
        }
        public static float? asFloat(this IDictionary<string, string> dictionary, string key, float? defaultValue)
        {
            float? result = defaultValue;

            if (dictionary.TryGetValue(key, out string value))
            {
                long resultValue;

                if (!long.TryParse(value, out resultValue))
                {
                    result = defaultValue;
                }
                else
                {
                    result = resultValue;
                }
            }

            return result;
        }

        public static bool? asBool(this IDictionary<string, string> dictionary, string key, bool? defaultValue)
        {
            bool? result = defaultValue;

            if (dictionary.TryGetValue(key, out string value))
            {
                bool resultValue;

                if (!bool.TryParse(value, out resultValue))
                {
                    result = defaultValue;
                }
                else
                {
                    result = resultValue;
                }
            }

            return result;
        }
    }
}
