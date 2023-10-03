using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Data.OnlineChecker;

public interface IDatabaseOnlineChecker
{
    protected const int MAXIMUM_ATTEMPTS_TO_CONNECT = 10;
    protected const int RETRY_AFTER_MILLISEC = 5000;
    
    bool IsDbOnline(IConfiguration configuration);
}