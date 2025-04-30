using System;

namespace OpenBudgeteer.Core.Common.Extensions;

public static class DateOnlyExtensions
{
    public static bool IsBetween(this DateOnly date, DateOnly? start, DateOnly? end) =>
        date >= (start ?? DateOnly.MinValue) && 
        date <= (end ?? DateOnly.MaxValue);
    
    public static bool IsBetween(this DateOnly date, DateTime? start, DateTime? end) =>
        date >= DateOnly.FromDateTime(start ?? DateTime.MinValue) && 
        date <= DateOnly.FromDateTime(end ?? DateTime.MaxValue);
}