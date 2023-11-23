using System;
using OpenBudgeteer.Core.Common.Extensions;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class MappingRuleViewModel : BaseEntityViewModel<MappingRule>
{
    #region Properties & Fields
    
    public enum MappingComparisionField
    {
        [StringValue("Account")]
        Account = 1,
        [StringValue("Payee")]
        Payee = 2,
        [StringValue("Memo")]
        Memo = 3,
        [StringValue("Amount")]
        Amount = 4
    }

    public enum MappingComparisionType
    {
        [StringValue("Equal")]
        Equal = 1,
        [StringValue("Not equal")]
        NotEqual = 2,
        [StringValue("Contains")]
        Contains = 3,
        [StringValue("Does not contain")]
        DoesNotContain = 4
    }
    
    /// <summary>
    /// Database Id of the MappingRule
    /// </summary>
    public readonly Guid MappingRuleId;
    
    /// <summary>
    /// Database Id of the RuleSet to which this MappingRule is assigned to 
    /// </summary>
    public readonly Guid BucketRuleSetId;
    
    private MappingComparisionField _comparisionField;
    /// <summary>
    /// To which property of <see cref="BankTransaction"/> this MappingRule should apply
    /// </summary>
    public MappingComparisionField ComparisionField 
    { 
        get => _comparisionField;
        set => Set(ref _comparisionField, value);
    }

    /// <summary>
    /// Output of <see cref="ComparisionField"/> used for display purposes
    /// </summary>
    public string ComparisonFieldOutput
    {
        get
        {
            return ComparisionField switch
            {
                MappingComparisionField.Account => "Account",
                MappingComparisionField.Payee => nameof(MappingComparisionField.Payee),
                MappingComparisionField.Memo => nameof(MappingComparisionField.Memo),
                MappingComparisionField.Amount => nameof(MappingComparisionField.Amount),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
    private MappingComparisionType _comparisionType;
    /// <summary>
    /// Identifier how comparison should happen
    /// </summary>
    public MappingComparisionType ComparisionType 
    { 
        get => _comparisionType;
        set => Set(ref _comparisionType, value);
    }

    /// <summary>
    /// Output of <see cref="ComparisionType"/> used for display purposes
    /// </summary>
    public string ComparisionTypeOutput
    {
        get
        {
            return ComparisionType switch
            {
                MappingComparisionType.Equal => nameof(MappingComparisionType.Equal).ToLower(),
                MappingComparisionType.NotEqual => CamelCaseConverter.ConvertToSpaces(nameof(MappingComparisionType.NotEqual)).ToLower(),
                MappingComparisionType.Contains => nameof(MappingComparisionType.Contains).ToLower(),
                MappingComparisionType.DoesNotContain => CamelCaseConverter.ConvertToSpaces(nameof(MappingComparisionType.DoesNotContain)).ToLower(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
    private string _comparisionValue;
    /// <summary>
    /// Value which should be used for the comparision
    /// </summary>
    public string ComparisionValue 
    { 
        get => _comparisionValue;
        set => Set(ref _comparisionValue, value);
    }

    /// <summary>
    /// Helper property to generate a readable output for <see cref="MappingRule"/>
    /// </summary>
    public string RuleOutput => $"{ComparisonFieldOutput} " +
                                $"{ComparisionTypeOutput} " +
                                $"{ComparisionValue}";
    
    #endregion
    
    #region Constructors

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="MappingRule"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="mappingRule">MappingRule instance</param>
    public MappingRuleViewModel(IServiceManager serviceManager, MappingRule mappingRule) : base(serviceManager)
    {
        MappingRuleId = mappingRule.Id;
        BucketRuleSetId = mappingRule.BucketRuleSetId;
        _comparisionField = (MappingComparisionField)mappingRule.ComparisionField;
        _comparisionType = (MappingComparisionType)mappingRule.ComparisionType;
        _comparisionValue = mappingRule.ComparisionValue;
    }
    
    #endregion
    
    #region Modification Handler

    internal override MappingRule ConvertToDto()
    {
        return new MappingRule()
        {
            Id = MappingRuleId,
            BucketRuleSetId = BucketRuleSetId,
            ComparisionField = (int)ComparisionField,
            ComparisionType = (int)ComparisionType,
            ComparisionValue = ComparisionValue
        };
    }
    
    #endregion
}