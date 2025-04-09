using System.Threading.Tasks;

namespace OpenBudgeteer.Blazor.Common.Services;

public class AppSettingService
{
    public record AppSettings(
        int HomePageTopCount,
        int TransactionPagerSize,
        ReportCounts ReportCounts);

    public record ReportCounts(
        int MonthBalance,
        int BankBalance,
        int MonthIncomeExpenses,
        int YearIncomeExpenses,
        int MonthBucketExpenses);

    public AppSettings CurrentSettings { get; private set; }
    
    private readonly RedisService _redisService;

    private const string HOME_PAGE_TOP_COUNT = "HomePage:TopCount";
    private const string REPORT_PAGE_MONTH_BALANCE_COUNT = "ReportPage:MonthBalanceCount";
    private const string REPORT_PAGE_BANK_BALANCE_COUNT = "ReportPage:BankBalanceCount";
    private const string REPORT_PAGE_MONTH_INCOME_EXPENSES_COUNT = "ReportPage:MonthIncomeExpensesCount";
    private const string REPORT_PAGE_YEAR_INCOME_EXPENSES_COUNT = "ReportPage:YearIncomeExpensesCount";
    private const string REPORT_PAGE_MONTH_BUCKET_EXPENSES_COUNT = "ReportPage:MonthBucketExpensesCount";
    private const string TRANSACTION_PAGER_SIZE = "Transaction:PageSize"; 
    
    private const int HOME_PAGE_TOP_COUNT_DEFAULT = 5;
    private const int REPORT_PAGE_MONTH_BALANCE_COUNT_DEFAULT = 24;
    private const int REPORT_PAGE_BANK_BALANCE_COUNT_DEFAULT = 24;
    private const int REPORT_PAGE_MONTH_INCOME_EXPENSES_COUNT_DEFAULT = 24;
    private const int REPORT_PAGE_YEAR_INCOME_EXPENSES_COUNT_DEFAULT = 5;
    private const int REPORT_PAGE_MONTH_BUCKET_EXPENSES_COUNT_DEFAULT = 12;
    private const int TRANSACTION_PAGER_SIZE_DEFAULT = 25;
    
    public AppSettingService(RedisService redisService)
    {
        _redisService = redisService;
        CurrentSettings = new(
            HOME_PAGE_TOP_COUNT_DEFAULT,
            TRANSACTION_PAGER_SIZE_DEFAULT,
            new ReportCounts(
                REPORT_PAGE_MONTH_BALANCE_COUNT_DEFAULT,
                REPORT_PAGE_BANK_BALANCE_COUNT_DEFAULT,
                REPORT_PAGE_MONTH_INCOME_EXPENSES_COUNT_DEFAULT,
                REPORT_PAGE_YEAR_INCOME_EXPENSES_COUNT_DEFAULT,
                REPORT_PAGE_MONTH_BUCKET_EXPENSES_COUNT_DEFAULT));
    }
    
    public async Task InitializeAsync()
    {
        CurrentSettings = await GetSettingsAsync();
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        return new AppSettings(
            await GetRedisIntValueAsync(HOME_PAGE_TOP_COUNT, HOME_PAGE_TOP_COUNT_DEFAULT),
            await GetRedisIntValueAsync(TRANSACTION_PAGER_SIZE, TRANSACTION_PAGER_SIZE_DEFAULT),
            new ReportCounts(
                await GetRedisIntValueAsync(REPORT_PAGE_MONTH_BALANCE_COUNT, REPORT_PAGE_MONTH_BALANCE_COUNT_DEFAULT),
                await GetRedisIntValueAsync(REPORT_PAGE_BANK_BALANCE_COUNT, REPORT_PAGE_BANK_BALANCE_COUNT_DEFAULT),
                await GetRedisIntValueAsync(REPORT_PAGE_MONTH_INCOME_EXPENSES_COUNT, REPORT_PAGE_MONTH_INCOME_EXPENSES_COUNT_DEFAULT),
                await GetRedisIntValueAsync(REPORT_PAGE_YEAR_INCOME_EXPENSES_COUNT, REPORT_PAGE_YEAR_INCOME_EXPENSES_COUNT_DEFAULT),
                await GetRedisIntValueAsync(REPORT_PAGE_MONTH_BUCKET_EXPENSES_COUNT, REPORT_PAGE_MONTH_BUCKET_EXPENSES_COUNT_DEFAULT)));
        
        async Task<int> GetRedisIntValueAsync(string key, int defaultValue)
        {
            return int.TryParse(await _redisService.GetStringValueAsync(key), out var parsedValue) 
                ? parsedValue : defaultValue;
        }
    }

    public AppSettings GetDefaultSettings()
    {
        return new AppSettings(
            HOME_PAGE_TOP_COUNT_DEFAULT,
            TRANSACTION_PAGER_SIZE_DEFAULT,
            new ReportCounts(
                REPORT_PAGE_MONTH_BALANCE_COUNT_DEFAULT,
                REPORT_PAGE_BANK_BALANCE_COUNT_DEFAULT,
                REPORT_PAGE_MONTH_INCOME_EXPENSES_COUNT_DEFAULT,
                REPORT_PAGE_YEAR_INCOME_EXPENSES_COUNT_DEFAULT,
                REPORT_PAGE_MONTH_BUCKET_EXPENSES_COUNT_DEFAULT));
    }
    
    public async Task ApplySettingsAsync(AppSettings newSettings)
    {
        await Task.WhenAll(
            _redisService.SetStringValueAsync(HOME_PAGE_TOP_COUNT, newSettings.HomePageTopCount.ToString()),
            _redisService.SetStringValueAsync(TRANSACTION_PAGER_SIZE, newSettings.TransactionPagerSize.ToString()), 
            _redisService.SetStringValueAsync(REPORT_PAGE_MONTH_BALANCE_COUNT, newSettings.ReportCounts.MonthBalance.ToString()), 
            _redisService.SetStringValueAsync(REPORT_PAGE_BANK_BALANCE_COUNT, newSettings.ReportCounts.BankBalance.ToString()), 
            _redisService.SetStringValueAsync(REPORT_PAGE_MONTH_INCOME_EXPENSES_COUNT, newSettings.ReportCounts.MonthIncomeExpenses.ToString()), 
            _redisService.SetStringValueAsync(REPORT_PAGE_YEAR_INCOME_EXPENSES_COUNT, newSettings.ReportCounts.YearIncomeExpenses.ToString()), 
            _redisService.SetStringValueAsync(REPORT_PAGE_MONTH_BUCKET_EXPENSES_COUNT, newSettings.ReportCounts.MonthBucketExpenses.ToString()));
        
        CurrentSettings = newSettings;
    }
    
    public async Task ResetSettingsAsync()
    {
        await Task.WhenAll(
            _redisService.DeleteKeyAsync(HOME_PAGE_TOP_COUNT),
            _redisService.DeleteKeyAsync(TRANSACTION_PAGER_SIZE),
            _redisService.DeleteKeyAsync(REPORT_PAGE_MONTH_BALANCE_COUNT),
            _redisService.DeleteKeyAsync(REPORT_PAGE_BANK_BALANCE_COUNT),
            _redisService.DeleteKeyAsync(REPORT_PAGE_MONTH_INCOME_EXPENSES_COUNT),
            _redisService.DeleteKeyAsync(REPORT_PAGE_YEAR_INCOME_EXPENSES_COUNT),
            _redisService.DeleteKeyAsync(REPORT_PAGE_MONTH_BUCKET_EXPENSES_COUNT));
    }
}