using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBudgetedTransactionService : IBaseService<BudgetedTransaction>
{
    public IEnumerable<BudgetedTransaction> GetAll(DateTime periodStart, DateTime periodEnd);
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId);
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId, DateTime periodStart, DateTime periodEnd);
    public IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId);
    public IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId, DateTime periodStart, DateTime periodEnd);
    public IEnumerable<BudgetedTransaction> GetAllNonTransfer();
    public IEnumerable<BudgetedTransaction> GetAllNonTransfer(DateTime periodStart, DateTime periodEnd);
    public IEnumerable<BudgetedTransaction> GetAllTransfer();
    public IEnumerable<BudgetedTransaction> GetAllTransfer(DateTime periodStart, DateTime periodEnd);
    public IEnumerable<BudgetedTransaction> GetAllIncome();
    public IEnumerable<BudgetedTransaction> GetAllIncome(DateTime periodStart, DateTime periodEnd);
}