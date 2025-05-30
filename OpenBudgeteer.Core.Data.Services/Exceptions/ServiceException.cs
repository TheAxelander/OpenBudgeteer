using Microsoft.Extensions.Logging;

namespace OpenBudgeteer.Core.Data.Services.Exceptions;

public class ServiceException : Exception
{
    public ServiceException(string message, ILogger logger) : base(message)
    {
        logger.LogError(message);
    }
}