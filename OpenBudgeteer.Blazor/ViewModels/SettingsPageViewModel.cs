using System;
using System.Threading.Tasks;
using MudBlazor;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.AppSettings;
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

    private int _homePageTopCount;
    /// <summary>
    /// Sets how many records should be displayed on Home Page for Hightest Incomes and Highest Expenses
    /// </summary>
    public int HomePageTopCount
    {
        get => _homePageTopCount;
        set => Set(ref _homePageTopCount, value);
    }

    private int _transactionPageSize;
    /// <summary>
    /// Sets how many Transactions should be displayed per page
    /// </summary>
    public int TransactionPageSize
    {
        get => _transactionPageSize;
        set => Set(ref _transactionPageSize, value);
    }

    private int _reportPageMonthBalanceCount;
    /// <summary>
    /// Sets for how many months data in the Month Balance Chart should be shown
    /// </summary>
    public int ReportPageMonthBalanceCount
    {
        get => _reportPageMonthBalanceCount;
        set => Set(ref _reportPageMonthBalanceCount, value);
    }

    private int _reportPageBankBalanceCount;
    /// <summary>
    /// Sets for how many months data in the Bank Balance Chart should be shown
    /// </summary>
    public int ReportPageBankBalanceCount
    {
        get => _reportPageBankBalanceCount;
        set => Set(ref _reportPageBankBalanceCount, value);
    }

    private int _reportPageMonthIncomeExpensesCount;
    /// <summary>
    /// Sets for how many months data in the Month Income & Expenses Chart should be shown
    /// </summary>
    public int ReportPageMonthIncomeExpensesCount
    {
        get => _reportPageMonthIncomeExpensesCount;
        set => Set(ref _reportPageMonthIncomeExpensesCount, value);
    }

    private int _reportPageYearIncomeExpensesCount;
    /// <summary>
    /// Sets for how many years data in the Year Income & Expenses Chart should be shown
    /// </summary>
    public int ReportPageYearIncomeExpensesCount
    {
        get => _reportPageYearIncomeExpensesCount;
        set => Set(ref _reportPageYearIncomeExpensesCount, value);
    }

    private int _reportPageMonthBucketExpensesCount;
    /// <summary>
    /// Sets for how many months data in the Month Bucket Expenses Charts should be shown
    /// </summary>
    public int ReportPageMonthBucketExpensesCount
    {
        get => _reportPageMonthBucketExpensesCount;
        set => Set(ref _reportPageMonthBucketExpensesCount, value);
    }

    private readonly MudThemeService _mudThemeService;
    private readonly IAppSettingService _appSettingService;

    public SettingsPageViewModel(
        IServiceManager serviceManager,
        MudThemeService mudThemeService,
        IAppSettingService appSettingService)
        : base(serviceManager, serviceManager.CreateLogger(typeof(SettingsPageViewModel)))
    {
        _mudThemeService = mudThemeService;
        _appSettingService = appSettingService;
        _currentTheme = new MudTheme();
        _currentPalette = CurrentTheme.PaletteLight;
    }

    /// <summary>
    /// Reloads Data from Redis and resets setting values
    /// </summary>
    public async Task<ViewModelOperationResult> LoadDataAsync()
    {
        try
        {
            // Theme settings
            var themeSetting = await _mudThemeService.GetThemeAsync();
            CurrentTheme = themeSetting.CurrentTheme;
            UseDarkTheme = themeSetting.IsDarkMode;
            CurrentPalette = UseDarkTheme ? CurrentTheme.PaletteDark : CurrentTheme.PaletteLight;

            // Other settings
            var otherSettings = await _appSettingService.GetSettingsAsync();
            HomePageTopCount = otherSettings.HomePageTopCount;
            TransactionPageSize = otherSettings.TransactionPagerSize;
            ReportPageMonthBalanceCount = otherSettings.ReportCounts.MonthBalance;
            ReportPageBankBalanceCount = otherSettings.ReportCounts.BankBalance;
            ReportPageMonthIncomeExpensesCount = otherSettings.ReportCounts.MonthIncomeExpenses;
            ReportPageYearIncomeExpensesCount = otherSettings.ReportCounts.YearIncomeExpenses;
            ReportPageMonthBucketExpensesCount = otherSettings.ReportCounts.MonthBucketExpenses;
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
            await _mudThemeService.ApplyThemeAsync(new(CurrentTheme, UseDarkTheme));
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

    /// <summary>
    /// Takes all miscellaneous settings and stores them to the Redis database
    /// </summary>
    public async Task<ViewModelOperationResult> ApplySettingsAsync()
    {
        try
        {
            await _appSettingService.ApplySettingsAsync(new(
                HomePageTopCount,
                TransactionPageSize,
                new (
                    ReportPageMonthBalanceCount,
                    ReportPageBankBalanceCount,
                    ReportPageMonthIncomeExpensesCount,
                    ReportPageYearIncomeExpensesCount,
                    ReportPageMonthBucketExpensesCount)));
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error on saving settings: {e.Message}");
        }
        return new ViewModelOperationResult(true);
    }
}
