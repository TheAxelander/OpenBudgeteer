namespace OpenBudgeteer.Core.Data.Connection;

/// <summary>
/// Database connection details for display purposes, independent of the used database access technology
/// </summary>
public record DatabaseConnectionInfo(string Provider, string Server, string Database, string Username);
