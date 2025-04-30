using System;
using TinyCsvParser.Mapping;

namespace OpenBudgeteer.Blazor.Common;

/// <summary>
/// Custom class to make <see cref="CsvMappingError"/> equatable against the pure error message
/// </summary>
public class EquatableCsvMappingError : IEquatable<EquatableCsvMappingError>
{
    public string Value => _csvMappingError.Value;
    public string UnmappedRow => _csvMappingError.UnmappedRow;
    
    private readonly CsvMappingError _csvMappingError;

    public EquatableCsvMappingError(CsvMappingError csvMappingError)
    {
        _csvMappingError = csvMappingError;
    }
    
    #region IEquatable Implementation
    
    public bool Equals(EquatableCsvMappingError? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _csvMappingError.Value == other._csvMappingError.Value; 
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EquatableCsvMappingError)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(_csvMappingError.Value);
        return hashCode.ToHashCode();
    }

    public override string ToString() => _csvMappingError.Value;

    #endregion
}