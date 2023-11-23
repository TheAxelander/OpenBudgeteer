using System.Linq.Expressions;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Repositories;

public interface IBankTransactionRepository : IBaseRepository<BankTransaction>
{
}