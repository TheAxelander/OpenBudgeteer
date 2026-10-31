using System.Threading.Tasks;

namespace OpenBudgeteer.Core.Common.AppSettings;

public class BaseAppSettingService : IAppSettingService
{
    public AppSettings CurrentSettings { get; protected set; }

    // Setting key names
    protected virtual string HomePageTopCountKey => "HOME_PAGE_TOP_COUNT";
    protected virtual string TransactionPagerSizeKey => "TRANSACTION_PAGE_SIZE";
    protected virtual string ReportPageMonthBalanceCountKey => "REPORT_PAGE_MONTH_BALANCE_COUNT";
    protected virtual string ReportPageBankBalanceCountKey => "REPORT_PAGE_BANK_BALANCE_COUNT";
    protected virtual string ReportPageMonthIncomeExpensesCountKey => "REPORT_PAGE_MONTH_INCOME_EXPENSES_COUNT";
    protected virtual string ReportPageYearIncomeExpensesCountKey => "REPORT_PAGE_YEAR_INCOME_EXPENSES_COUNT";
    protected virtual string ReportPageMonthBucketExpensesCountKey => "REPORT_PAGE_MONTH_BUCKET_EXPENSES_COUNT";

    // Default values
    protected virtual int HomePageTopCountDefault => 5;
    protected virtual int TransactionPagerSizeDefault => 25;
    protected virtual int ReportPageMonthBalanceCountDefault => 24;
    protected virtual int ReportPageBankBalanceCountDefault => 24;
    protected virtual int ReportPageMonthIncomeExpensesCountDefault => 24;
    protected virtual int ReportPageYearIncomeExpensesCountDefault => 5;
    protected virtual int ReportPageMonthBucketExpensesCountDefault => 12;

    private readonly ISettingHandler _settingHandler;

    protected BaseAppSettingService(ISettingHandler settingHandler)
    {
        _settingHandler = settingHandler;
        CurrentSettings = GetDefaultSettings();
    }

    private AppSettings GetDefaultSettings()
    {
        return new AppSettings(
            HomePageTopCountDefault,
            TransactionPagerSizeDefault,
            new ReportCounts(
                ReportPageMonthBalanceCountDefault,
                ReportPageBankBalanceCountDefault,
                ReportPageMonthIncomeExpensesCountDefault,
                ReportPageYearIncomeExpensesCountDefault,
                ReportPageMonthBucketExpensesCountDefault));
    }

    public virtual async Task InitializeAsync()
    {
        CurrentSettings = await GetSettingsAsync();
    }

    public virtual async Task<AppSettings> GetSettingsAsync()
    {
        return new AppSettings(
            await _settingHandler.GetIntValueAsync(HomePageTopCountKey, HomePageTopCountDefault),
            await _settingHandler.GetIntValueAsync(TransactionPagerSizeKey, TransactionPagerSizeDefault),
            new ReportCounts(
                await _settingHandler.GetIntValueAsync(ReportPageMonthBalanceCountKey, ReportPageMonthBalanceCountDefault),
                await _settingHandler.GetIntValueAsync(ReportPageBankBalanceCountKey, ReportPageBankBalanceCountDefault),
                await _settingHandler.GetIntValueAsync(ReportPageMonthIncomeExpensesCountKey, ReportPageMonthIncomeExpensesCountDefault),
                await _settingHandler.GetIntValueAsync(ReportPageYearIncomeExpensesCountKey, ReportPageYearIncomeExpensesCountDefault),
                await _settingHandler.GetIntValueAsync(ReportPageMonthBucketExpensesCountKey, ReportPageMonthBucketExpensesCountDefault)));
    }

    public virtual async Task ApplySettingsAsync(AppSettings newSettings)
    {
        await Task.WhenAll(
            _settingHandler.SetIntValueAsync(HomePageTopCountKey, newSettings.HomePageTopCount),
            _settingHandler.SetIntValueAsync(TransactionPagerSizeKey, newSettings.TransactionPagerSize),
            _settingHandler.SetIntValueAsync(ReportPageMonthBalanceCountKey, newSettings.ReportCounts.MonthBalance),
            _settingHandler.SetIntValueAsync(ReportPageBankBalanceCountKey, newSettings.ReportCounts.BankBalance),
            _settingHandler.SetIntValueAsync(ReportPageMonthIncomeExpensesCountKey, newSettings.ReportCounts.MonthIncomeExpenses),
            _settingHandler.SetIntValueAsync(ReportPageYearIncomeExpensesCountKey, newSettings.ReportCounts.YearIncomeExpenses),
            _settingHandler.SetIntValueAsync(ReportPageMonthBucketExpensesCountKey, newSettings.ReportCounts.MonthBucketExpenses));

        CurrentSettings = newSettings;
    }

    public virtual async Task ResetSettingsAsync()
    {
        await Task.WhenAll(
            _settingHandler.DeleteKeyAsync(HomePageTopCountKey),
            _settingHandler.DeleteKeyAsync(TransactionPagerSizeKey),
            _settingHandler.DeleteKeyAsync(ReportPageMonthBalanceCountKey),
            _settingHandler.DeleteKeyAsync(ReportPageBankBalanceCountKey),
            _settingHandler.DeleteKeyAsync(ReportPageMonthIncomeExpensesCountKey),
            _settingHandler.DeleteKeyAsync(ReportPageYearIncomeExpensesCountKey),
            _settingHandler.DeleteKeyAsync(ReportPageMonthBucketExpensesCountKey));
        CurrentSettings = GetDefaultSettings();
    }
}
