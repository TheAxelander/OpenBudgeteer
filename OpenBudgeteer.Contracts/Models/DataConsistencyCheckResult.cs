using System.Collections.Generic;

namespace OpenBudgeteer.Contracts.Models;

public class DataConsistencyCheckResult 
{
    public enum StatusCode { Ok, Warning, Alert }

    public string CheckName { get; }
    public StatusCode Status { get; }
    public string Message { get; }
    public List<string[]> Details { get; }

    public DataConsistencyCheckResult(string checkName, StatusCode status, string message, List<string[]> details) 
    {
        CheckName = checkName;
        Status = status;
        Message = message;
        Details = details;
    }
}
