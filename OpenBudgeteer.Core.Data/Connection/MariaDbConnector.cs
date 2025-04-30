using System;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using OpenBudgeteer.Core.Data.Entities;

namespace OpenBudgeteer.Core.Data.Connection;

public partial class MariaDbConnector : BaseDatabaseConnector<MySqlConnectionStringBuilder>
{
    public override string Provider => "MariaDB/MySql";
    
    public MariaDbConnector(IConfiguration configuration) : base(configuration)
    {
        if (string.IsNullOrEmpty(Server)) Server = "localhost";
        if (Port == 0) Port = 3306;
        
        if (string.IsNullOrEmpty(Database)) Database = "openbudgeteer";
        if (!DatabaseNameRegex().IsMatch(Database))
        {
            throw new InvalidOperationException("Database name provided is illegal or SQLi attempt");
        }
        
        if (string.IsNullOrEmpty(Username)) Username = Database;
        if (!DatabaseNameRegex().IsMatch(Username))
        {
            throw new InvalidOperationException("User name provided is illegal or SQLi attempt");
        }
    }

    public override MySqlConnectionStringBuilder BuildConnectionString()
    {
        return new MySqlConnectionStringBuilder
        {
            Server = Server,
            Port = (uint)Port,
            Database = Database,
            UserID = Username,
            Password = Password,
            ConnectionProtocol = MySqlConnectionProtocol.Tcp
        };
    }

    public override MySqlConnectionStringBuilder BuildRootConnectionString()
    {
        return new MySqlConnectionStringBuilder
        {
            Server = Server,
            Port = (uint)Port,
            UserID = "root",
            Password = RootPassword,
            ConnectionProtocol = MySqlConnectionProtocol.Tcp
        };
    }

    protected override DbContextOptionsBuilder<DatabaseContext> BuildDbContextOptions()
    {
        var builder = new DbContextOptionsBuilder<DatabaseContext>();
        var connectionStringBuilder = BuildConnectionString();
        
        var serverVersion = ServerVersion.AutoDetect(connectionStringBuilder.ConnectionString);
        return builder.UseMySql(
            connectionStringBuilder.ConnectionString,
            serverVersion,
            b => b.MigrationsAssembly("OpenBudgeteer.Core.Data.MySql.Migrations"));
    }

    public override bool IsDatabaseAccessible(bool useRoot = false)
    {
        try
        {
            var connectionStringBuilder = useRoot ? BuildRootConnectionString() : BuildConnectionString();
            using var connection = new MySqlConnection(connectionStringBuilder.ConnectionString);
            connection.Open();
                
            using var command = new MySqlCommand("SELECT 1");
            command.Connection = connection;
            command.CommandType = CommandType.Text;

            command.ExecuteScalar();
            connection.Close();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Database not accessible: { e.Message }");
            return false;
        }
    }

    [GeneratedRegex("^[a-zA-Z][0-9a-zA-Z$_-]{0,63}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex DatabaseNameRegex();
}