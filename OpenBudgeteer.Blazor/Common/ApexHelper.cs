using ApexCharts;

namespace OpenBudgeteer.Blazor.Common;

public class ApexHelper
{
    public static void BalanceChartMutator(DataPoint<ReportRecord> point)
    {
        point.FillColor = point.Y > 0
            ? System.Drawing.Color.Green.ToHexString()
            : System.Drawing.Color.DarkRed.ToHexString();
    }
}