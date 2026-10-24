namespace OpenBudgeteer.Core.Common.AppSettings;

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
