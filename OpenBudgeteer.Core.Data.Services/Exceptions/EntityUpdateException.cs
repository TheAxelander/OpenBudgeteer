namespace OpenBudgeteer.Core.Data.Services.Exceptions;

public class EntityUpdateException : Exception
{
    public EntityUpdateException() { }
    public EntityUpdateException(string message) : base(message) { }
}