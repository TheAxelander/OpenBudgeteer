using MudBlazor;
using MudBlazor.Utilities;
using OpenBudgeteer.Blazor.Common.Services;

namespace OpenBudgeteer.Photino.Services;

public class PhotinoMudThemeService : IMudThemeService
{
    public MudThemeSetting CurrentThemeSetting { get; private set; }

    public event EventHandler<MudThemeSetting>? ThemeChanged;

    private readonly ConfigFileService _configService;

    // Config keys
    private const string THEME_IS_DARK_MODE = "THEME_IS_DARK_MODE";
    private const string THEME_LIGHT_PREFIX = "THEME_LIGHT_";
    private const string THEME_DARK_PREFIX = "THEME_DARK_";

    public PhotinoMudThemeService(ConfigFileService configService)
    {
        _configService = configService;
        CurrentThemeSetting = new MudThemeSetting(new MudTheme(), false);
    }

    public async Task InitializeAsync()
    {
        CurrentThemeSetting = await GetThemeAsync();
    }

    public async Task<MudThemeSetting> GetThemeAsync()
    {
        return await Task.Run(() =>
        {
            var theme = GetDefaultTheme();
            var isDarkMode = _configService.GetBoolValue(THEME_IS_DARK_MODE, false);

            UpdatePalette(THEME_LIGHT_PREFIX, theme.PaletteLight);
            UpdatePalette(THEME_DARK_PREFIX, theme.PaletteDark);

            return new MudThemeSetting(theme, isDarkMode);
        });
    }

    public MudTheme GetDefaultTheme()
    {
        var result = new MudTheme();
        result.Typography.Button.TextTransform = "none";
        return result;
    }

    private void UpdatePalette(string prefix, Palette palette)
    {
        var entries = _configService.GetSettingsByPrefix(prefix);

        foreach (var (key, value) in entries)
        {
            if (string.IsNullOrEmpty(value)) continue;

            var setting = key.Replace(prefix, string.Empty);
            SetColorProperty(setting, new MudColor(value));
        }

        return;

        void SetColorProperty(string setting, MudColor newColor)
        {
            var colorProperty = typeof(Palette).GetProperty(setting);
            if (colorProperty == null) return; // Palette Color Property doesn't exist
            if (colorProperty.PropertyType != typeof(MudColor)) return; // Not matching Type

            colorProperty.SetValue(palette, newColor);
        }
    }

    public async Task ApplyThemeAsync(MudThemeSetting newSetting)
    {
        _configService.SetStringValue(THEME_IS_DARK_MODE, newSetting.IsDarkMode.ToString());

        var lightPaletteSettings = BuildPaletteSettings(THEME_LIGHT_PREFIX, newSetting.CurrentTheme.PaletteLight, new PaletteLight());
        var darkPaletteSettings = BuildPaletteSettings(THEME_DARK_PREFIX, newSetting.CurrentTheme.PaletteDark, new PaletteDark());

        // Remove old palette settings first
        _configService.DeleteKeysByPrefix(THEME_LIGHT_PREFIX);
        _configService.DeleteKeysByPrefix(THEME_DARK_PREFIX);

        // Set new settings
        _configService.SetValues(lightPaletteSettings);
        _configService.SetValues(darkPaletteSettings);

        await _configService.SaveAsync();

        CurrentThemeSetting = newSetting;

        ThemeChanged?.Invoke(this, CurrentThemeSetting);
    }

    private static Dictionary<string, string> BuildPaletteSettings(string prefix, Palette currentPalette, Palette defaultPalette)
    {
        var settings = new Dictionary<string, string>();

        IdentifyCustomSetting(nameof(currentPalette.Primary));
        IdentifyCustomSetting(nameof(currentPalette.Secondary));
        IdentifyCustomSetting(nameof(currentPalette.Tertiary));
        IdentifyCustomSetting(nameof(currentPalette.Info));
        IdentifyCustomSetting(nameof(currentPalette.Success));
        IdentifyCustomSetting(nameof(currentPalette.Warning));
        IdentifyCustomSetting(nameof(currentPalette.Error));
        IdentifyCustomSetting(nameof(currentPalette.Dark));

        IdentifyCustomSetting(nameof(currentPalette.AppbarText));
        IdentifyCustomSetting(nameof(currentPalette.AppbarBackground));
        IdentifyCustomSetting(nameof(currentPalette.DrawerText));
        IdentifyCustomSetting(nameof(currentPalette.DrawerIcon));
        IdentifyCustomSetting(nameof(currentPalette.DrawerBackground));

        IdentifyCustomSetting(nameof(currentPalette.Surface));
        IdentifyCustomSetting(nameof(currentPalette.Background));
        IdentifyCustomSetting(nameof(currentPalette.BackgroundGray));
        IdentifyCustomSetting(nameof(currentPalette.LinesDefault));
        IdentifyCustomSetting(nameof(currentPalette.LinesInputs));
        IdentifyCustomSetting(nameof(currentPalette.Divider));
        IdentifyCustomSetting(nameof(currentPalette.DividerLight));

        IdentifyCustomSetting(nameof(currentPalette.TextPrimary));
        IdentifyCustomSetting(nameof(currentPalette.TextSecondary));
        IdentifyCustomSetting(nameof(currentPalette.TextDisabled));
        IdentifyCustomSetting(nameof(currentPalette.ActionDefault));
        IdentifyCustomSetting(nameof(currentPalette.ActionDisabled));
        IdentifyCustomSetting(nameof(currentPalette.ActionDisabledBackground));

        return settings;

        void IdentifyCustomSetting(string setting)
        {
            var colorProperty = typeof(Palette).GetProperty(setting);
            if (colorProperty == null) return; // Palette Color Property doesn't exist

            var currentColor = colorProperty.GetValue(currentPalette) as MudColor;
            var defaultColor = colorProperty.GetValue(defaultPalette) as MudColor;

            if (currentColor is null) return; // Unable to parse Color Property
            if (currentColor == defaultColor) return; // No custom setting, hence skip

            settings.Add($"{prefix}{setting}", $"\"{currentColor.ToString(MudColorOutputFormats.HexA)}\"");
        }
    }

    public async Task ResetThemeAsync()
    {
        _configService.DeleteKeysByPrefix(THEME_LIGHT_PREFIX);
        _configService.DeleteKeysByPrefix(THEME_DARK_PREFIX);
        _configService.DeleteKey(THEME_IS_DARK_MODE);

        await _configService.SaveAsync();

        CurrentThemeSetting = new MudThemeSetting(GetDefaultTheme(), false);
        ThemeChanged?.Invoke(this, CurrentThemeSetting);
    }
}
