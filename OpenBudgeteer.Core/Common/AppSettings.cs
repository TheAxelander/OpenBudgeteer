using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Core.Common;

public class AppSettings
{
    public enum ThemeMode { Light, Dark }
    
    private const string APPSETTINGS_THEME = "APPSETTINGS_THEME";
    
    public string Theme { get; set; }

    public ThemeMode Mode { get; set; }

    public bool IsDarkMode
    {
        get => Mode == ThemeMode.Dark;
        set => Mode = value ? ThemeMode.Dark : ThemeMode.Light;
    }

    public AppSettings(ConfigurationManager configuration)
    {
        Mode = configuration.GetValue(APPSETTINGS_THEME, "dark") == "dark" ? ThemeMode.Dark : ThemeMode.Light;
        Theme = configuration.GetValue(APPSETTINGS_THEME, "default") ?? "default";
    }
}
