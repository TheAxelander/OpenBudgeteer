using System;
using System.Threading.Tasks;
using MudBlazor;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels;

namespace OpenBudgeteer.Blazor.ViewModels;

public class SettingsPageViewModel : ViewModelBase
{
    private MudTheme _currentTheme;
    /// <summary>
    /// Instance of the currently used <see cref="MudTheme"/>
    /// </summary>
    public MudTheme CurrentTheme
    {
        get => _currentTheme;
        private set
        {
            Set(ref _currentTheme, value);
            CurrentPalette = UseDarkTheme ? value.PaletteDark : value.PaletteLight;
        } 
    }
    
    private Palette _currentPalette;
    /// <summary>
    /// Instance of the currently used Color Palette
    /// </summary>
    public Palette CurrentPalette
    {
        get => _currentPalette;
        private set => Set(ref _currentPalette, value);
    }
    
    //public Palette CurrentPalette => UseDarkTheme ? CurrentTheme.PaletteDark : CurrentTheme.PaletteLight;
    
    private bool _useDarkTheme;
    /// <summary>
    /// Identifies if the Dark Theme and Palette should be used
    /// </summary>
    public bool UseDarkTheme
    {
        get => _useDarkTheme;
        set
        {
            Set(ref _useDarkTheme, value);
            CurrentPalette = value ? CurrentTheme.PaletteDark : CurrentTheme.PaletteLight;
        }
    }
    
    private readonly MudThemeService _mudThemeService;

    public SettingsPageViewModel(IServiceManager serviceManager, MudThemeService mudThemeService) : base(serviceManager)
    {
        _mudThemeService = mudThemeService;
        CurrentTheme = new MudTheme();
        CurrentPalette = CurrentTheme.PaletteLight;
    }

    /// <summary>
    /// Reloads Data from Redis and resets setting values
    /// </summary>
    public async Task<ViewModelOperationResult> LoadDataAsync()
    {
        try
        {
            CurrentTheme = await _mudThemeService.GetThemeAsync();
            UseDarkTheme = _mudThemeService.IsDarkMode;
            CurrentPalette = UseDarkTheme ? CurrentTheme.PaletteDark : CurrentTheme.PaletteLight;
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
        }
        return new ViewModelOperationResult(true);
    }

    /// <summary>
    /// Takes all defined Themes settings and stores them to the Redis database
    /// </summary>
    public async Task<ViewModelOperationResult> ApplyThemeAsync()
    {
        try
        {
            _mudThemeService.IsDarkMode = UseDarkTheme;
            await _mudThemeService.ApplyThemeAsync(CurrentTheme);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error on saving settings: {e.Message}");
        }
        return new ViewModelOperationResult(true);
    }

    /// <summary>
    /// Creates a new <see cref="MudTheme"/> instance for <see cref="CurrentTheme"/> with its default values
    /// and deletes all Color settings from Redis (except DarkMode setting)
    /// </summary>
    public async Task RestoreDefaultThemeAsync()
    {
        CurrentTheme = _mudThemeService.GetDefaultTheme();
        await _mudThemeService.ResetThemeAsync();
        await ApplyThemeAsync();
    }
}