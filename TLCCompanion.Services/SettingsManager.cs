using System.Text.Json;
using TLCCompanion.Interfaces;
using TLCCompanion.Models;

namespace TLCCompanion.Services
{
    public class SettingsManager : ISettingsManager
    {
        private AppConfig _settings = new AppConfig();

        // Start of Constructors region

        #region Constructors

        public SettingsManager()
        {
            LoadSettings();
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public AppConfig Settings { get => _settings; set => _settings = value; }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        #endregion

        // Start of Methods region

        #region Methods

        public void LoadSettings()
        {
            string fileName = "Config/Settings.json";
            if (File.Exists(fileName))
            {
                using FileStream stream = File.OpenRead(fileName);
                _settings = JsonSerializer.Deserialize<AppConfig>(stream) ?? new AppConfig();
            }

            SaveSettings();
        }

        public void SaveSettings()
        {
            string fileName = "Config/Settings.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            JsonSerializer.Serialize(stream, _settings, options);
        }

        #endregion
    }
}