using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Models;

public class Account : BaseObject
{
    private Guid _accountId;
    public Guid AccountId
    {
        get => _accountId;
        set => Set(ref _accountId, value);
    }

    private string _name;
    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    private int _isActive;
    public int IsActive
    {
        get => _isActive;
        set => Set(ref _isActive, value);
    }
}
