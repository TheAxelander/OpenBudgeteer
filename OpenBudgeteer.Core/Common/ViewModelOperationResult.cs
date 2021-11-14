namespace OpenBudgeteer.Core.Common;

public class ViewModelOperationResult
{
    public bool IsSuccessful { get; }
    public string Message { get; }
    public bool ViewModelReloadRequired { get; }

    public ViewModelOperationResult(bool isSuccessful, string message, bool viewModelReloadRequired = false)
    {
        IsSuccessful = isSuccessful;
        Message = message;
        ViewModelReloadRequired = viewModelReloadRequired;
    }

    public ViewModelOperationResult(bool isSuccessful, bool viewModelReloadInvoked = false) 
        : this(isSuccessful, string.Empty, viewModelReloadInvoked)
    {
        if (!isSuccessful)
        {
            Message = "Unknown Error.";
        }
    }
}
