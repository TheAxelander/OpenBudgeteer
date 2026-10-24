using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Dapper;
using DuckDB.NET.Data;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.DuckDb.Migrations;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb;

namespace OpenBudgeteer.Core.Test.Tests.Database;

public abstract class BaseDatabaseTest<TEntity> where TEntity : IEntity
{
    private static bool _dapperConfigured;

    // protected static DbContextOptions MariaDbContextOptions
    // {
    //     get
    //     {
    //         var configuration = new ConfigurationBuilder()
    //             .AddInMemoryCollection(new Dictionary<string, string>
    //             {
    //                 [ConfigurationKeyConstants.CONNECTION_PROVIDER] = "mariadb",
    //                 [ConfigurationKeyConstants.CONNECTION_SERVER] = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.CONNECTION_SERVER) ?? "192.168.178.153",
    //                 [ConfigurationKeyConstants.CONNECTION_PORT] = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.CONNECTION_PORT) ?? "3306",
    //                 [ConfigurationKeyConstants.CONNECTION_USER] = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.CONNECTION_USER) ?? "openbudgeteer_unit_test",
    //                 [ConfigurationKeyConstants.CONNECTION_PASSWORD] = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.CONNECTION_PASSWORD) ?? "openbudgeteer_unit_test",
    //                 [ConfigurationKeyConstants.CONNECTION_DATABASE] = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.CONNECTION_DATABASE) ?? "openbudgeteer_unit_test",
    //             }!)
    //             .Build();
    //         var dbContextOptionsBuilder = new DbContextOptionsBuilder();
    //         var contextOptions = new MariaDbConnector(configuration, dbContextOptionsBuilder).BuildDbConnection();
    //         var dbContext = new DatabaseContext(contextOptions);
    //         dbContext.Database.Migrate();
    //         return contextOptions;
    //     }
    // }
    
    protected static DatabaseContext GetEFCoreInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new DatabaseContext(options);

        // Optional: Seed data here
        context.Database.EnsureCreated();

        return context;
    }

    protected static DbConnection GetDuckDbInMemoryConnection()
    {
        // Configure Dapper type handlers for DuckDB
        if (!_dapperConfigured)
        {
            SqlMapper.AddTypeHandler(new DuckDbGuidTypeHandler());
            _dapperConfigured = true;
        }

        var connection = new DuckDBConnection("DataSource=:memory:");
        connection.Open();

        // Apply migrations to set up schema and initial data
        DuckDbMigrationRunner.ApplyMigrations(connection);

        return connection;
    }
    
    protected abstract void CompareEntities(TEntity expected, TEntity actual);

    protected virtual void RunChecks(IBaseRepository<TEntity> baseRepository, List<TEntity> testEntities)
    {
        var dbEntities = baseRepository.All().ToList();
        //Assert.Equal(4, dbEntity.Count);
        foreach (var testEntity in testEntities)
        {
            var dbEntity = dbEntities.First(i => i.Id == testEntity.Id);
            CompareEntities(testEntity, dbEntity);
        }
    }
}