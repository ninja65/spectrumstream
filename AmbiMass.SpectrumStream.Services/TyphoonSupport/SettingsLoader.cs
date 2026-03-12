using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace AmbiMass.SpectrumStream.Services.TyphoonSupport
{
    public static class SettingsLoader
    {
        public static MSSettings LoadSettings(string settingsFile)
        {
            var loadedSettings = File.Exists(settingsFile) ? JsonSerializer.Deserialize<MSSettings>(File.ReadAllText(settingsFile)) : new MSSettings();

            return loadedSettings;
        }

    }
}