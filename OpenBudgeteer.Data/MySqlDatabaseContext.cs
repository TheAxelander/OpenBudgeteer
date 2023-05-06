using Microsoft.EntityFrameworkCore;

namespace OpenBudgeteer.Data;

public class MySqlDatabaseContext : DatabaseContext
{
    private const string CharacterSet = "utf8mb4";
    
    public MySqlDatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCharSet(CharacterSet);
        
        base.OnModelCreating(modelBuilder);
    }
}
