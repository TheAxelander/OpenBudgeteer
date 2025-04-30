using System;
using OpenBudgeteer.Core.Common.Extensions;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class MappingRuleViewModel : BaseEntityViewModel<MappingRule>, IEquatable<MappingRuleViewModel>, IComparable<MappingRuleViewModel>
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
    /// Value which should be used for the comparison
    /// </summary>
    public string ComparisonValue 
    { 
        get => _comparisonValue;
        set => Set(ref _comparisonValue, value);
    }

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
        _comparisonField = (MappingRuleComparisonField)mappingRule.ComparisonField;
        _comparisonType = (MappingRuleComparisonType)mappingRule.ComparisonType;
        _comparisonValue = mappingRule.ComparisonValue;
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
            ComparisonField = (int)ComparisonField,
            ComparisonType = (int)ComparisonType,
            ComparisonValue = ComparisonValue
        };
    }
    
    #endregion

    #region IEquatable & IComparable Implementation

    public bool Equals(MappingRuleViewModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return 
            MappingRuleId.Equals(other.MappingRuleId) && 
            BucketRuleSetId.Equals(other.BucketRuleSetId) && 
            _comparisonField == other._comparisonField && 
            _comparisonType == other._comparisonType && 
            _comparisonValue == other._comparisonValue;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((MappingRuleViewModel)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(MappingRuleId);
        hashCode.Add(BucketRuleSetId);
        hashCode.Add((int)_comparisonField);
        hashCode.Add((int)_comparisonType);
        hashCode.Add(_comparisonValue);
        return hashCode.ToHashCode();
    }
    
    public int CompareTo(MappingRuleViewModel? other)
    {
        return string.Compare(ToString(), other?.ToString(), StringComparison.Ordinal);
    }

    public override string ToString() => $"{ComparisonField.GetStringValue()} " +
                                         $"{ComparisonType.GetStringValue()} " +
                                         $"{ComparisonValue}";

    #endregion
}