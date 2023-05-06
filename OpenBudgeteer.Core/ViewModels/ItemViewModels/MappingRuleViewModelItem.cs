using Microsoft.EntityFrameworkCore;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels;

public class MappingRuleViewModelItem : ViewModelBase
{
    private MappingRule _mappingRule;
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public MappingRule MappingRule
    {
        get => _mappingRule;
        set => Set(ref _mappingRule, value);
    }

    private string _ruleOutput;
    /// <summary>
    /// Helper property to generate a readable output for <see cref="MappingRule"/>
    /// </summary>
    public string RuleOutput
    {
        get => _ruleOutput;
        set => Set(ref _ruleOutput, value);
    }
    
    private readonly DbContextOptions<DatabaseContext> _dbOptions;

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    public MappingRuleViewModelItem(DbContextOptions<DatabaseContext> dbOptions)
    {
        _dbOptions = dbOptions;
    }

    /// <summary>
    /// Initialize ViewModel with an existing <see cref="MappingRule"/> object
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="mappingRule">MappingRule instance</param>
    public MappingRuleViewModelItem(DbContextOptions<DatabaseContext> dbOptions, MappingRule mappingRule) : this(dbOptions)
    {
        MappingRule = mappingRule;
        GenerateRuleOutput();
    }

    /// <summary>
    /// Translates <see cref="MappingRule"/> object into a readable format
    /// </summary>
    public void GenerateRuleOutput()
    {
        RuleOutput = MappingRule == null ? string.Empty :
            $"{MappingRule.ComparisonFieldOutput} " +
            $"{MappingRule.ComparisionTypeOutput} " +
            $"{MappingRule.ComparisionValue}";
    }
}
