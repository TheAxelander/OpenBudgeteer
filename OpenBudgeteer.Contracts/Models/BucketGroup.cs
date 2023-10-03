using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBudgeteer.Contracts.Models;

public class BucketGroup : BaseObject
{
    private Guid _bucketGroupId;
    public Guid BucketGroupId
    {
        get => _bucketGroupId;
        set => Set(ref _bucketGroupId, value);
    }

    private string _name;
    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    private int _position;
    [Required]
    public int Position
    {
        get => _position;
        set => Set(ref _position, value);
    }
}
