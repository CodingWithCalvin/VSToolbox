using System.Text.Json;
using Microsoft.Win32;

namespace CodingWithCalvin.VSToolbox.Services;

public class SettingsService
{
    private const string StartupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "VSToolbox";
    private const string SettingsFileName = "settings.json";

    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CodingWithCalvin.VSToolbox",
        SettingsFileName);

    private static SettingsData? _settings;
    private static readonly object _lock = new();

    private static SettingsData Settings
    {
        get
        {
            if (_settings is null)
            {
                lock (_lock)
                {
                    _settings ??= LoadSettings();
                }
            }
            return _settings;
        }
    }

    public bool LaunchOnStartup
    {
        get => Settings.LaunchOnStartup;
        set
        {
            Settings.LaunchOnStartup = value;
            SaveSettings();
            UpdateStartupRegistration(value);
        }
    }

    public bool StartMinimized
    {
        get => Settings.StartMinimized;
        set
        {
            Settings.StartMinimized = value;
            SaveSettings();
        }
    }

    public bool MinimizeToTray
    {
        get => Settings.MinimizeToTray;
        set
        {
            Settings.MinimizeToTray = value;
            SaveSettings();
        }
    }

    public bool CloseToTray
    {
        get => Settings.CloseToTray;
        set
        {
            Settings.CloseToTray = value;
            SaveSettings();
        }
    }

    private static SettingsData LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
            }
        }
        catch
        {
            // If we can't load settings, return defaults
        }
        return new SettingsData();
    }

    private static void SaveSettings()
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch
        {
            // Silently fail if we can't save settings
        }
    }

    private static void UpdateStartupRegistration(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, writable: true);
            if (key is null) return;

            if (enable)
            {
                var exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
            }
            else
            {
                key.DeleteValue(AppName, throwOnMissingValue: false);
            }
        }
        catch
        {
            // Silently fail if we can't access the registry
        }
    }

    /// <summary>
    /// Checks if the app is currently registered for startup and syncs the setting.
    /// Call this on app startup to ensure settings match actual state.
    /// </summary>
    public void SyncStartupSetting()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, writable: false);
            var isRegistered = key?.GetValue(AppName) is not null;

            // Update setting to match registry state
            if (Settings.LaunchOnStartup != isRegistered)
            {
                Settings.LaunchOnStartup = isRegistered;
                SaveSettings();
            }
        }
        catch
        {
            // Silently fail
        }
    }

    private class SettingsData
    {
        public bool LaunchOnStartup { get; set; }
        public bool StartMinimized { get; set; }
        public bool MinimizeToTray { get; set; } = true;
        public bool CloseToTray { get; set; } = true;
    }
}
