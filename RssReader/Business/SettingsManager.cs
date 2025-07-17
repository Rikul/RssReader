using RssReader.Data;
using RssReader.Models;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RssReader.Business
{
    public class SettingsManager
    {
        private readonly SettingsRepository _settingsRepository;
        private Settings _currentSettings;
        
        public SettingsManager(DatabaseContext context)
        {
            _settingsRepository = new SettingsRepository(context);
        }
        
        public async Task<Settings> LoadSettingsAsync()
        {
            _currentSettings = await _settingsRepository.GetSettingsAsync();
            return _currentSettings;
        }
        
        public async Task<Settings> SaveSettingsAsync(Settings settings)
        {
            await _settingsRepository.UpdateSettingsAsync(settings);
            _currentSettings = settings;
            
            // Apply startup setting
            SetStartWithWindows(settings.StartWithWindows);
            
            return settings;
        }
        
        public Settings GetCurrentSettings()
        {
            return _currentSettings;
        }
        
        private void SetStartWithWindows(bool startWithWindows)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (startWithWindows)
                {
                    string appPath = System.Reflection.Assembly.GetEntryAssembly().Location;
                    key.SetValue("RssReader", $"\"{appPath}\"");
                }
                else
                {
                    if (key.GetValue("RssReader") != null)
                    {
                        key.DeleteValue("RssReader", false);
                    }
                }
            }
        }
    }
}