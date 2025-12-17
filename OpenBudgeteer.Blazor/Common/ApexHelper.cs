using System.Collections.Generic;
using ApexCharts;
using OpenBudgeteer.Blazor.Common.Extensions;

namespace OpenBudgeteer.Blazor.Common;

public record ApexRecord(string Label, decimal Value);
public record BucketApexRecord(string Name, List<ApexRecord> Records); // Records needs to be a list as Apex RadialBar requires a List as Items source
public record BucketGroupApexRecord(string Name, List<BucketApexRecord> Buckets);

public class ApexHelper
{
    public static void BalanceChartMutator(DataPoint<ApexRecord> point)
    {
        point.FillColor = point.Y > 0
            ? System.Drawing.Color.Green.ToHexString()
            : System.Drawing.Color.DarkRed.ToHexString();
    }
}