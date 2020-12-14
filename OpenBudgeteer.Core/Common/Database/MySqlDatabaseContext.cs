using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpenBudgeteer.Core.Common.Database
{
    public class MySqlDatabaseContext : DatabaseContext
    {
        public MySqlDatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
    }
}
