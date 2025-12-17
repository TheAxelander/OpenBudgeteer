using System;
using System.Data;
using Dapper;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb;

/// <summary>
/// Custom Dapper type handler to map DuckDB VARCHAR to/from GUID
/// </summary>
public class DuckDbGuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override Guid Parse(object value)
    {
        if (value == null || value is DBNull)
            return Guid.Empty;

        if (value is string str)
            return Guid.Parse(str);

        if (value is Guid guid)
            return guid;

        throw new InvalidCastException($"Cannot convert {value.GetType()} to Guid");
    }

    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString();
        parameter.DbType = DbType.String;
    }
}