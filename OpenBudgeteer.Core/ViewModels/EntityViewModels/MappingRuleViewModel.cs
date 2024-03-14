using System;
using OpenBudgeteer.Core.Common.Extensions;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class MappingRuleViewModel : BaseEntityViewModel<MappingRule>
{
    #region Properties & Fields
    
    /// <summary>
    /// Database Id of the MappingRule
    /// </summary>
    public readonly Guid MappingRuleId;
    
    /// <summary>
    /// Database Id of the RuleSet to which this MappingRule is assigned to 
    /// </summary>
    public readonly Guid BucketRuleSetId;
    
    private MappingRuleComparisonField _comparisonField;
    /// <summary>
    /// To which property of <see cref="BankTransaction"/> this MappingRule should apply
    /// </summary>
    public MappingRuleComparisonField ComparisonField 
    { 
        get => _comparisonField;
        set => Set(ref _comparisonField, value);
    }

    private MappingRuleComparisonType _comparisonType;
    /// <summary>
    /// Identifier how comparison should happen
    /// </summary>
    public MappingRuleComparisonType ComparisonType 
    { 
        get => _comparisonType;
        set => Set(ref _comparisonType, value);
    }

    private string _comparisonValue;
    /// <summary>
    /// Value which should be used for the comparision
    /// </summary>
    public string ComparisonValue 
    { 
        get => _comparisonValue;
        set => Set(ref _comparisonValue, value);
    }

    /// <summary>
    /// Helper property to generate a readable output for <see cref="MappingRule"/>
    /// </summary>
    public string RuleOutput => $"{ComparisonField.GetStringValue()} " +
                                $"{ComparisonType.GetStringValue()} " +
                                $"{ComparisonValue}";
    
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
        _comparisonField = (MappingRuleComparisonField)mappingRule.ComparisionField;
        _comparisonType = (MappingRuleComparisonType)mappingRule.ComparisionType;
        _comparisonValue = mappingRule.ComparisionValue;
    }

    /// <summary>
    /// Initialize a copy of the passed ViewModel
    /// </summary>
    /// <param name="viewModel">Current ViewModel instance</param>
    protected MappingRuleViewModel(MappingRuleViewModel viewModel) : base(viewModel.ServiceManager)
    {
        MappingRuleId = viewModel.MappingRuleId;
        BucketRuleSetId = viewModel.BucketRuleSetId;
        _comparisonField = viewModel.ComparisonField;
        _comparisonType = viewModel.ComparisonType;
        _comparisonValue = viewModel.ComparisonValue;
    }

    /// <summary>
    /// Return a deep copy of the ViewModel
    /// </summary>
    public override object Clone()
    {
        return new MappingRuleViewModel(this);
    }

    #endregion
    
    #region Modification Handler

    internal override MappingRule ConvertToDto()
    {
        return new MappingRule()
        {
            Id = MappingRuleId,
            BucketRuleSetId = BucketRuleSetId,
            ComparisionField = (int)ComparisonField,
            ComparisionType = (int)ComparisonType,
            ComparisionValue = ComparisonValue
        };
    }
    
    #endregion
}