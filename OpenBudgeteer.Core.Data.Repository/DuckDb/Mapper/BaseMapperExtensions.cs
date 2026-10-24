namespace OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

public static class BaseMapperExtensions
{
    internal static void AddWithBackReference<TChild>(
        ICollection<TChild> collection,
        TChild? child,
        Func<TChild, Guid> idSelector,
        Action<TChild> setBackReference) where TChild : class
    {
        if (child == null) return;
        if (collection.Any(c => idSelector(c) == idSelector(child))) return;

        collection.Add(child);
        setBackReference.Invoke(child);
    }
}
