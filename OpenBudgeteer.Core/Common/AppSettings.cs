using OpenBudgeteer.Core.Common.Extensions;

namespace OpenBudgeteer.Core.Common;

public static class AppSettings
{
    public enum ThemeMode { Light, Dark }
    
    public static string Theme { get; set; } = "default";

    public static ThemeMode Mode => Theme switch
    {
        "cyborg" or 
        "darkly" or 
        "slate" or 
        "solar" or 
        "superhero" or 
        "vapor" => ThemeMode.Dark,
        _ => ThemeMode.Light
    };
}
