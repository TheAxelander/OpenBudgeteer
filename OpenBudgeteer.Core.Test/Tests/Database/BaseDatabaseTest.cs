using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenBudgeteer.Core.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using Xunit;

namespace OpenBudgeteer.Core.Test.Tests.Database;

public abstract class BaseDatabaseTest<TEntity> where TEntity : IEntity
{
    protected static DbContextOptions<DatabaseContext> MariaDbContextOptions
    {
        get
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    [ConfigurationKeyConstants.CONNECTION_PROVIDER] = "mariadb",
                    [ConfigurationKeyConstants.CONNECTION_SERVER] = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.CONNECTION_SERVER) ?? "192.168.178.153",
                    [ConfigurationKeyConstants.CONNECTION_PORT] = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.CONNECTION_PORT) ?? "3306",
                    [ConfigurationKeyConstants.CONNECTION_USER] = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.CONNECTION_USER) ?? "openbudgeteer_unit_test",
                    [ConfigurationKeyConstants.CONNECTION_PASSWORD] = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.CONNECTION_PASSWORD) ?? "openbudgeteer_unit_test",
                    [ConfigurationKeyConstants.CONNECTION_DATABASE] = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.CONNECTION_DATABASE) ?? "openbudgeteer_unit_test",
                }!)
                .Build();
            var contextOptions = DbContextOptionsFactory.GetContextOptions(configuration);
            var dbContext = new DatabaseContext(contextOptions);
            dbContext.Database.Migrate();
            return contextOptions;
        }
    }
    
    protected abstract void CompareEntities(TEntity expected, TEntity actual);
    // public abstract void Create(IBaseRepository<TEntity> baseRepository);
    // public abstract void Update(IBaseRepository<TEntity> baseRepository);
    // public abstract void Delete(IBaseRepository<TEntity> baseRepository);

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