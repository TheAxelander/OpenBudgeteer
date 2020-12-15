using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBudgeteer.Core.Common
{
    public class ViewModelOperationResult
    {
        public bool IsSuccessful { get; }
        public string Message { get; }
        public bool ViewModelReloadInvoked { get; }

        public ViewModelOperationResult(bool isSuccessful, string message, bool viewModelReloadInvoked = false)
        {
            IsSuccessful = isSuccessful;
            Message = message;
            ViewModelReloadInvoked = viewModelReloadInvoked;
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
}
