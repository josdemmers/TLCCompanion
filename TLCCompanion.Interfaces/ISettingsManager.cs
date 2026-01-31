using TLCCompanion.Models;

namespace TLCCompanion.Interfaces
{
    public interface ISettingsManager
    {
        AppConfig Settings { get; }

        void LoadSettings();
        void SaveSettings();
    }
}
