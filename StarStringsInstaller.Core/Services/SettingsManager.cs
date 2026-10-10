using System;
using System.IO;
using System.Text.Json;

namespace StarStringsInstaller.Services
{
    public class SettingsManager
    {
        private readonly string _settingsFilePath;

        public SettingsManager(string? customFilePath = null)
        {
            if (!string.IsNullOrEmpty(customFilePath))
            {
                _settingsFilePath = customFilePath;
            }
            else
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string folder = Path.Combine(appData, "StarStringsInstaller");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                _settingsFilePath = Path.Combine(folder, "settings.json");
            }
        }

        public class AppSettings
        {
            public string StarCitizenPath { get; set; } = string.Empty;
            public string InstalledVersion { get; set; } = "None";
            public string TargetEnvironmentPath { get; set; } = string.Empty;
        }

        public AppSettings Load()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null) return settings;
                }
            }
            catch
            {
                // Fallback to default
            }

            return new AppSettings();
        }

        public void Save(AppSettings settings)
        {
            try
            {
                var dir = Path.GetDirectoryName(_settingsFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch
            {
                // Ignore save errors if permissions restricted
            }
        }
    }
}
