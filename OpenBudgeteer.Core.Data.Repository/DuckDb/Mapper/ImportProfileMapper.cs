using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

public static class ImportProfileMapperExtensions
{
    public static ImportProfile MapWithAccount(this ImportProfile importProfile, Account account)
    {
        importProfile.Account = account;
        return importProfile;
    }
}

public class ImportProfileMapper
{
    public IEnumerable<ImportProfile> Results => _cache.Values;

    private readonly Dictionary<Guid, ImportProfile> _cache = new();

    public ImportProfile MapWithEverything(ImportProfile importProfile, Account account)
    {
        var result = GetOrAdd(importProfile);
        return result.MapWithAccount(account);
    }

    private ImportProfile GetOrAdd(ImportProfile importProfile)
    {
        if (_cache.TryGetValue(importProfile.Id, out var existing))
            return existing;

        existing = importProfile;
        _cache.Add(importProfile.Id, existing);

        return existing;
    }
}
