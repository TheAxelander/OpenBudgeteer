using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBudgetedTransactionService : IBaseService<BudgetedTransaction>
{
    public IEnumerable<BudgetedTransaction> GetAll(DateOnly periodStart, DateOnly periodEnd);
    public IEnumerable<BudgetedTransaction> GetAllForReporting(DateOnly periodStart, DateOnly periodEnd);
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId);
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId, DateOnly periodStart, DateOnly periodEnd);
    public IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId);
    public IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId, DateOnly periodStart, DateOnly periodEnd);
    public IEnumerable<BudgetedTransaction> GetAllNonTransfer();
    public IEnumerable<BudgetedTransaction> GetAllNonTransfer(DateOnly periodStart, DateOnly periodEnd);
    public IEnumerable<BudgetedTransaction> GetAllTransfer();
    public IEnumerable<BudgetedTransaction> GetAllTransfer(DateOnly periodStart, DateOnly periodEnd);
    public IEnumerable<BudgetedTransaction> GetAllIncome();
    public IEnumerable<BudgetedTransaction> GetAllIncome(DateOnly periodStart, DateOnly periodEnd);
}