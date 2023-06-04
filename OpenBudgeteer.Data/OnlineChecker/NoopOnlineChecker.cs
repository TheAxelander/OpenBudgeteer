using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Data.OnlineChecker;

// SQLite will come on line with the container :)
public class NoopOnlineChecker : IDatabaseOnlineChecker
{
    public bool IsDbOnline(IConfiguration configuration)
    {
        return true;
    }
}