using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreAccountService : EFCoreBaseService<Account>, IAccountService
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;

    public EFCoreAccountService(DbContextOptions<DatabaseContext> dbContextOptions) : base(dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }

    protected override GenericAccountService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericAccountService(
            new AccountRepository(dbContext),
            new BankTransactionRepository(dbContext));
    }
    
    public IEnumerable<Account> GetActiveAccounts()
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var genericAccountService = CreateBaseService(dbContext);
            return genericAccountService.GetActiveAccounts();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public Account CloseAccount(Guid id)
    {
        using var dbContext = new DatabaseContext(_dbContextOptions);
        var genericAccountService = CreateBaseService(dbContext);
        return genericAccountService.CloseAccount(id);
    }
}