using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Core.Data.OnlineChecker;

public class NoopOnlineChecker : IDatabaseOnlineChecker
{
    public bool IsDbOnline(IConfiguration configuration)
    {
        return true;
    }
}