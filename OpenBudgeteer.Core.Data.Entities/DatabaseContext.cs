using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Entities;

public class DatabaseContext : DbContext
{
    public DbSet<Account> Account { get; set; }
    public DbSet<BankTransaction> BankTransaction { get; set; }
    public DbSet<RecurringBankTransaction> RecurringBankTransaction { get; set; }
    public DbSet<Bucket> Bucket { get; set; }
    public DbSet<BucketGroup> BucketGroup { get; set; }
    public DbSet<BucketMovement> BucketMovement { get; set; }
    public DbSet<BucketVersion> BucketVersion { get; set; }
    public DbSet<BudgetedTransaction> BudgetedTransaction { get; set; }
    public DbSet<ImportProfile> ImportProfile { get; set; }
    public DbSet<BucketRuleSet> BucketRuleSet { get; set; }
    public DbSet<MappingRule> MappingRule { get; set; }
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
    
}