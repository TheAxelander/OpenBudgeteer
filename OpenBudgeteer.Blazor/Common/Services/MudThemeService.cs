using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MudBlazor;
using MudBlazor.Utilities;

namespace OpenBudgeteer.Blazor.Common.Services;

public class MudThemeService
{
    public record MudThemeSetting(MudTheme CurrentTheme, bool IsDarkMode);

    private static readonly Dictionary<string, Action<Palette, MudColor>> _paletteSetters = new()
    {
        ["Primary"] = (p, c) => p.Primary = c,
        ["Secondary"] = (p, c) => p.Secondary = c,
        ["Tertiary"] = (p, c) => p.Tertiary = c,
        ["Info"] = (p, c) => p.Info = c,
        ["Success"] = (p, c) => p.Success = c,
        ["Warning"] = (p, c) => p.Warning = c,
        ["Error"] = (p, c) => p.Error = c,
        ["Dark"] = (p, c) => p.Dark = c,
        ["AppbarText"] = (p, c) => p.AppbarText = c,
        ["AppbarBackground"] = (p, c) => p.AppbarBackground = c,
        ["DrawerText"] = (p, c) => p.DrawerText = c,
        ["DrawerIcon"] = (p, c) => p.DrawerIcon = c,
        ["DrawerBackground"] = (p, c) => p.DrawerBackground = c,
        ["Surface"] = (p, c) => p.Surface = c,
        ["Background"] = (p, c) => p.Background = c,
        ["BackgroundGray"] = (p, c) => p.BackgroundGray = c,
        ["LinesDefault"] = (p, c) => p.LinesDefault = c,
        ["LinesInputs"] = (p, c) => p.LinesInputs = c,
        ["Divider"] = (p, c) => p.Divider = c,
        ["DividerLight"] = (p, c) => p.DividerLight = c,
        ["TextPrimary"] = (p, c) => p.TextPrimary = c,
        ["TextSecondary"] = (p, c) => p.TextSecondary = c,
        ["TextDisabled"] = (p, c) => p.TextDisabled = c,
        ["ActionDefault"] = (p, c) => p.ActionDefault = c,
        ["ActionDisabled"] = (p, c) => p.ActionDisabled = c,
        ["ActionDisabledBackground"] = (p, c) => p.ActionDisabledBackground = c,
    };

    public MudThemeSetting CurrentThemeSetting { get; private set; }

    public event EventHandler<MudThemeSetting>? ThemeChanged;

    private readonly RedisService _redisService;

    private const string IS_DARK_MODE = "Theme:IsDarkMode";
    private const string PALETTE_LIGHT = "Theme:PaletteLight";
    private const string PALETTE_DARK = "Theme:PaletteDark";

    public MudThemeService(RedisService redisService)
    {
        _redisService = redisService;
        CurrentThemeSetting = new(new MudTheme(), false);
    }

    public async Task InitializeAsync()
    {
        CurrentThemeSetting = await GetThemeAsync();
    }

    public async Task<MudThemeSetting> GetThemeAsync()
    {
        var theme = GetDefaultTheme();
        var isDarkMode = await _redisService.GetBoolValueAsync(IS_DARK_MODE, false);

        await UpdatePaletteAsync(PALETTE_LIGHT, theme.PaletteLight);
        await UpdatePaletteAsync(PALETTE_DARK, theme.PaletteDark);

        return new(theme, isDarkMode);
    }

    public MudTheme GetDefaultTheme()
    {
        var result = new MudTheme();
        result.Typography.Button.TextTransform = "none";
        return result;
    }

    private async Task UpdatePaletteAsync(string redisKey, Palette palette)
    {
        var entries = await _redisService.GetHashEntriesAsync(redisKey);

        foreach (var entry in entries)
        {
            var entryValue = entry.Value.ToString();
            if (!string.IsNullOrEmpty(entryValue)) continue;

            if (_paletteSetters.TryGetValue(entryValue, out var colorSetter))
                colorSetter(palette, new MudColor(entryValue));
        }
    }

    public async Task ApplyThemeAsync(MudThemeSetting newSetting)
    {
        await _redisService.SetStringValueAsync(IS_DARK_MODE, newSetting.IsDarkMode.ToString());
        await _redisService.SetHashValueAsync(PALETTE_LIGHT, BuildPaletteHash(newSetting.CurrentTheme.PaletteLight, new PaletteLight()));
        await _redisService.SetHashValueAsync(PALETTE_DARK, BuildPaletteHash(newSetting.CurrentTheme.PaletteDark, new PaletteDark()));

        CurrentThemeSetting = newSetting;

        ThemeChanged?.Invoke(this, CurrentThemeSetting);
    }

    private Dictionary<string, string> BuildPaletteHash(Palette currentPalette, Palette defaultPalette)
    {
        var settings = new Dictionary<string, string>();

        if (currentPalette.Primary != defaultPalette.Primary) settings.Add("Primary", currentPalette.Primary.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.Secondary != defaultPalette.Secondary) settings.Add("Secondary", currentPalette.Secondary.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.Tertiary != defaultPalette.Tertiary) settings.Add("Tertiary", currentPalette.Tertiary.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.Info != defaultPalette.Info) settings.Add("Info", currentPalette.Info.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.Success != defaultPalette.Success) settings.Add("Success", currentPalette.Success.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.Warning != defaultPalette.Warning) settings.Add("Warning", currentPalette.Warning.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.Error != defaultPalette.Error) settings.Add("Error", currentPalette.Error.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.Dark != defaultPalette.Dark) settings.Add("Dark", currentPalette.Dark.ToString(MudColorOutputFormats.HexA));

        if (currentPalette.AppbarText != defaultPalette.AppbarText) settings.Add("AppbarText", currentPalette.AppbarText.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.AppbarBackground != defaultPalette.AppbarBackground) settings.Add("AppbarBackground", currentPalette.AppbarBackground.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.DrawerText != defaultPalette.DrawerText) settings.Add("DrawerText", currentPalette.DrawerText.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.DrawerIcon != defaultPalette.DrawerIcon) settings.Add("DrawerIcon", currentPalette.DrawerIcon.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.DrawerBackground != defaultPalette.DrawerBackground) settings.Add("DrawerBackground", currentPalette.DrawerBackground.ToString(MudColorOutputFormats.HexA));

        if (currentPalette.Surface != defaultPalette.Surface) settings.Add("Surface", currentPalette.Surface.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.Background != defaultPalette.Background) settings.Add("Background", currentPalette.Background.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.BackgroundGray != defaultPalette.BackgroundGray) settings.Add("BackgroundGray", currentPalette.BackgroundGray.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.LinesDefault != defaultPalette.LinesDefault) settings.Add("LinesDefault", currentPalette.LinesDefault.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.LinesInputs != defaultPalette.LinesInputs) settings.Add("LinesInputs", currentPalette.LinesInputs.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.Divider != defaultPalette.Divider) settings.Add("Divider", currentPalette.Divider.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.DividerLight != defaultPalette.DividerLight) settings.Add("DividerLight", currentPalette.DividerLight.ToString(MudColorOutputFormats.HexA));

        if (currentPalette.TextPrimary != defaultPalette.TextPrimary) settings.Add("TextPrimary", currentPalette.TextPrimary.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.TextSecondary != defaultPalette.TextSecondary) settings.Add("TextSecondary", currentPalette.TextSecondary.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.TextDisabled != defaultPalette.TextDisabled) settings.Add("TextDisabled", currentPalette.TextDisabled.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.ActionDefault != defaultPalette.ActionDefault) settings.Add("ActionDefault", currentPalette.ActionDefault.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.ActionDisabled != defaultPalette.ActionDisabled) settings.Add("ActionDisabled", currentPalette.ActionDisabled.ToString(MudColorOutputFormats.HexA));
        if (currentPalette.ActionDisabledBackground != defaultPalette.ActionDisabledBackground) settings.Add("ActionDisabledBackground", currentPalette.ActionDisabledBackground.ToString(MudColorOutputFormats.HexA));

        return settings;
    }

    public async Task ResetThemeAsync()
    {
        await _redisService.DeleteKeyAsync(PALETTE_LIGHT);
        await _redisService.DeleteKeyAsync(PALETTE_DARK);
    }
}
