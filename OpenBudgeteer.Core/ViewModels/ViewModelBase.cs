using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;

namespace OpenBudgeteer.Core.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    protected readonly IServiceManager ServiceManager;
    protected readonly ILogger Logger;

    protected ViewModelBase(IServiceManager serviceManager, ILogger logger)
    {
        ServiceManager = serviceManager;
        Logger = logger;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }
        field = value;
        NotifyPropertyChanged(propertyName);
        return true;
    }

    protected void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
