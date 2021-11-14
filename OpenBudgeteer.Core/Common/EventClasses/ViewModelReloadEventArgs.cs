using System;
using OpenBudgeteer.Core.ViewModels;

namespace OpenBudgeteer.Core.Common.EventClasses;

public class ViewModelReloadEventArgs : EventArgs
{
    public ViewModelBase ViewModel { get; private set; }

    public ViewModelReloadEventArgs(ViewModelBase viewModel)
    {
        ViewModel = viewModel;
    }
}
