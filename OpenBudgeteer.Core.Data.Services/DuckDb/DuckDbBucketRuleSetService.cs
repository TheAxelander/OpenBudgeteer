using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbBucketRuleSetService : GenericBucketRuleSetService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbBucketRuleSetService> _logger;

    public DuckDbBucketRuleSetService(
        Func<DbConnection> dbConnectionFactory,
        ILogger<DuckDbBucketRuleSetService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IBucketRuleSetRepository CreateBaseRepository(DbConnection dbConnection) => new DuckDbBucketRuleSetRepository(dbConnection);
    protected override IMappingRuleRepository CreateMappingRuleRepository(DbConnection dbConnection) => new DuckDbMappingRuleRepository(dbConnection);

    public override BucketRuleSet Update(BucketRuleSet entity)
    {
        using var dbContext = CreateDbConnection();
        using var transaction = dbContext.BeginTransaction();
        try
        {
            var bucketRuleSetRepository = CreateBaseRepository(dbContext);
            var mappingRuleRepository = CreateMappingRuleRepository(dbContext);

            // Check if Mapping Rules need to be deleted

            // Collect database entities
            var sql = """
                SELECT MappingRuleId AS Id
                FROM MappingRule
                WHERE BucketRuleSetId = $ruleSetId
                """;
            var existingMappingRuleIds = dbContext
                .Query<Guid>(sql, new { ruleSetId = entity.Id.ToString() })
                .ToList();

            // Select which of the database IDs are no longer available in entity
            var entityMappingRuleIds = entity.MappingRules?
                .Select(m => m.Id)
                .ToHashSet() ?? new HashSet<Guid>();
            var deletedIds = existingMappingRuleIds
                .Where(id => !entityMappingRuleIds.Contains(id))
                .ToList();

            if (deletedIds.Count != 0)
            {
                var result = mappingRuleRepository.DeleteRange(deletedIds);
                if (result != deletedIds.Count)
                    throw new EntityUpdateException("Unable to delete old Mapping Rules of that Rule Set");
            }

            // Update BucketRuleSet including MappingRules
            bucketRuleSetRepository.Update(entity);
            if (entity.MappingRules is not null && entity.MappingRules.Count != 0)
                mappingRuleRepository.CreateRange(entity.MappingRules);

            transaction.Commit();
            return entity;
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to update Rule Set: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public override void Delete(Guid id)
    {
        using var dbContext = CreateDbConnection();
        using var transaction = dbContext.BeginTransaction();
        try
        {
            var bucketRuleSetRepository = CreateBaseRepository(dbContext);
            var mappingRuleRepository = CreateMappingRuleRepository(dbContext);

            // Delete all existing Mapping Rules
            var sql = """
                SELECT MappingRuleId AS Id
                FROM MappingRule
                WHERE BucketRuleSetId = $ruleSetId
                """;
            mappingRuleRepository.DeleteRange(dbContext
                .Query<Guid>(sql, new { ruleSetId = id.ToString() })
                .ToList());

            // Delete BucketRuleSet
            bucketRuleSetRepository.Delete(id);
            transaction.Commit();
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to delete Rule Set: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}
