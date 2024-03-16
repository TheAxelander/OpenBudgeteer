using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenBudgeteer.Core.Data.Contracts.Services;

namespace OpenBudgeteer.Core.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    protected readonly IServiceManager ServiceManager;

    protected ViewModelBase(IServiceManager serviceManager)
    {
        ServiceManager = serviceManager;
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
