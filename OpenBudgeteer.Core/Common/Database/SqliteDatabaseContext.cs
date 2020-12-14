using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace OpenBudgeteer.Core.Common.Database
{
    public class SqliteDatabaseContext : DatabaseContext
    {
        public SqliteDatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            
        }
    }
}
