using OpenBudgeteer.Core.Common.AppSettings;

namespace OpenBudgeteer.Blazor.Common.Services;

public class RedisAppSettingService : BaseAppSettingService
{
    // Override key names for Redis-specific format
    protected override string HomePageTopCountKey => "HomePage:TopCount";
    protected override string TransactionPagerSizeKey => "Transaction:PageSize";
    protected override string ReportPageMonthBalanceCountKey => "ReportPage:MonthBalanceCount";
    protected override string ReportPageBankBalanceCountKey => "ReportPage:BankBalanceCount";
    protected override string ReportPageMonthIncomeExpensesCountKey => "ReportPage:MonthIncomeExpensesCount";
    protected override string ReportPageYearIncomeExpensesCountKey => "ReportPage:YearIncomeExpensesCount";
    protected override string ReportPageMonthBucketExpensesCountKey => "ReportPage:MonthBucketExpensesCount";

    public RedisAppSettingService(RedisService settingHandler) : base(settingHandler) { }
}
