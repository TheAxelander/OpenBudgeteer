using System.Collections.Concurrent;
using System.Text;
using OpenBudgeteer.Core.Common.AppSettings;

namespace OpenBudgeteer.Photino.Services;

/// <summary>
/// A file-based configuration service that reads and writes settings to an .env-like config file
/// </summary>
public class ConfigFileService : ISettingHandler
{
    /// <summary>
    /// Gets the directory where the config file is located
    /// </summary>
    public string ConfigDirectory => Path.GetDirectoryName(ConfigFilePath) ?? string.Empty;

    /// <summary>
    /// Gets the full path to the config file
    /// </summary>
    public string ConfigFilePath { get; }

    private readonly ConcurrentDictionary<string, string> _settings;

    public ConfigFileService(string? configFilePath = null)
    {
        ConfigFilePath = configFilePath ?? GetDefaultConfigPath();
        _settings = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        EnsureConfigFileExists();
        LoadSettings();
    }

    /// <summary>
    /// Gets the default config file path based on the used OS
    /// </summary>
    private static string GetDefaultConfigPath()
    {
        var configDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpenBudgeteer");

        /*if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            // For Linux/macOS: ~/.config/openbudgeteer/
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            configDirectory = Path.Combine(homeDir, ".config", "openbudgeteer");
        }
        else if (OperatingSystem.IsWindows())
        {
            // For Windows: %APPDATA%\OpenBudgeteer\
            configDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OpenBudgeteer");
        }
        else
        {
            // Fallback to LocalApplicationData
            configDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OpenBudgeteer");
        }*/

#if DEBUG
        configDirectory = Path.Combine(configDirectory, "dev");
#endif

        return Path.Combine(configDirectory, "openbudgeteer.conf");
    }

    /// <summary>
    /// Ensures the config file and its directory exists
    /// </summary>
    private void EnsureConfigFileExists()
    {
        if (string.IsNullOrEmpty(ConfigDirectory))
            throw new DirectoryNotFoundException("Unable to determine a config directory");

        if (!Directory.Exists(ConfigDirectory)) Directory.CreateDirectory(ConfigDirectory);
        if (!File.Exists(ConfigFilePath)) File.WriteAllText(ConfigFilePath, string.Empty);
    }

    /// <summary>
    /// Loads all settings from the config file
    /// </summary>
    private void LoadSettings()
    {
        var settings = DotNetEnv.Env
            .NoEnvVars()
            .Load(ConfigFilePath);
        foreach (var (key, value) in settings)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) continue;
            _settings[key] = value;
        }
    }

    /// <summary>
    /// Gets a string value from the configuration
    /// </summary>
    public string GetStringValue(string key, string defaultValue)
    {
        return _settings.GetValueOrDefault(key, defaultValue);
    }

    /// <summary>
    /// Gets a string value from the configuration
    /// </summary>
    public async Task<string> GetStringValueAsync(string key, string defaultValue)
    {
        return await Task.Run(() => GetStringValue(key, defaultValue));
    }

    /// <summary>
    /// Gets an integer value from the configuration
    /// </summary>
    public int GetIntValue(string key, int defaultValue)
    {
        return (_settings.TryGetValue(key, out var value) && int.TryParse(value, out var convertedValue))
            ? convertedValue
            : defaultValue;
    }

    /// <summary>
    /// Gets an integer value from the configuration
    /// </summary>
    public async Task<int> GetIntValueAsync(string key, int defaultValue)
    {
        return await Task.Run(() => GetIntValue(key, defaultValue));
    }

    /// <summary>
    /// Gets a boolean value from the configuration
    /// </summary>
    public bool GetBoolValue(string key, bool defaultValue)
    {
        return (_settings.TryGetValue(key, out var value) && bool.TryParse(value, out var convertedValue))
            ? convertedValue
            : defaultValue;
    }

    /// <summary>
    /// Gets a boolean value from the configuration
    /// </summary>
    public async Task<bool> GetBoolValueAsync(string key, bool defaultValue)
    {
        return await Task.Run(() => GetBoolValue(key, defaultValue));
    }

    /// <summary>
    /// Gets all settings with a specific prefix
    /// </summary>
    public Dictionary<string, string> GetSettingsByPrefix(string prefix)
    {
        return _settings
            .Where(i => i.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToDictionary();
    }

    /// <summary>
    /// Sets a string value in the configuration
    /// </summary>
    public void SetStringValue(string key, string value)
    {
        _settings[key] = value;
    }

    /// <summary>
    /// Sets a string value in the configuration
    /// </summary>
    public async Task SetStringValueAsync(string key, string value)
    {
        await Task.Run(() => SetStringValue(key, value));
    }

    /// <summary>
    /// Sets an integer value in the configuration
    /// </summary>
    public async Task SetIntValueAsync(string key, int value)
    {
        await Task.Run(() => SetStringValue(key, value.ToString()));
    }

    /// <summary>
    /// Sets a boolean value in the configuration
    /// </summary>
    public async Task SetBoolValueAsync(string key, bool value)
    {
        await  Task.Run(() => SetStringValue(key, value.ToString()));
    }

    /// <summary>
    /// Sets multiple string values in the configuration
    /// </summary>
    public void SetValues(Dictionary<string, string> settings)
    {
        foreach (var setting in settings)
        {
            _settings[setting.Key] = setting.Value;
        }
    }

    /// <summary>
    /// Removes a key from the configuration
    /// </summary>
    public void DeleteKey(string key)
    {
        _settings.TryRemove(key, out _);
    }

    /// <summary>
    /// Removes a key from the configuration
    /// </summary>
    public async Task DeleteKeyAsync(string key)
    {
        await Task.Run(() => DeleteKey(key));
    }

    /// <summary>
    /// Removes all keys with a specific prefix
    /// </summary>
    public void DeleteKeysByPrefix(string prefix)
    {
        var keysToRemove = _settings.Keys
            .Where(i => i.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _settings.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Checks if a key exists in the configuration
    /// </summary>
    public bool ContainsKey(string key)
    {
        return _settings.ContainsKey(key);
    }

    /// <summary>
    /// Checks if a key exists in the configuration
    /// </summary>
    public async Task<bool> ContainsKeyAsync(string key)
    {
        return await Task.Run(() => ContainsKeyAsync(key));
    }

    /// <summary>
    /// Saves all current settings to the config file
    /// </summary>
    public async Task SaveAsync()
    {
        var sb = new StringBuilder();
        foreach (var setting in _settings.OrderBy(x => x.Key))
        {
            sb.AppendLine($"{setting.Key}={setting.Value}");
        }
        await File.WriteAllTextAsync(ConfigFilePath, sb.ToString());
    }
}
