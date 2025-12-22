using System;
using System.Threading.Tasks;
using MudBlazor;

namespace OpenBudgeteer.Blazor.Common.Services;

public record MudThemeSetting(MudTheme CurrentTheme, bool IsDarkMode);

public interface IMudThemeService
{
    public MudThemeSetting CurrentThemeSetting { get; }

    public event EventHandler<MudThemeSetting>? ThemeChanged;

    public Task InitializeAsync();
    public Task<MudThemeSetting> GetThemeAsync();
    public MudTheme GetDefaultTheme();
    public Task ApplyThemeAsync(MudThemeSetting newSetting);
    public Task ResetThemeAsync();
}
