namespace OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

internal static class BaseMapperExtensions
{
    internal static void AddIfNotNull<T>(ICollection<T> list, T? item, Func<T, Guid> idSelector) where T : class
    {
        if (item != null && list.All(x => idSelector(x) != idSelector(item)))
            list.Add(item);
    }
}
