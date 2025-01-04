namespace OpenBudgeteer.Core.Data.Services.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException() { }
    public EntityNotFoundException(string message) : base(message) { }
}