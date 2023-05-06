using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Contracts.Models;

public class BucketVersion : BaseObject
{
    private Guid _bucketVersionId;
    public Guid BucketVersionId
    {
        get => _bucketVersionId;
        set => Set(ref _bucketVersionId, value);
    }

    private Guid _bucketId;
    [Required]
    public Guid BucketId
    {
        get => _bucketId;
        set => Set(ref _bucketId, value);
    }

    private Bucket _bucket;
    public Bucket Bucket
    {
        get => _bucket;
        set => Set(ref _bucket, value);
    }

    private int _version;
    [Required]
    public int Version
    {
        get => _version;
        set => Set(ref _version, value);
    }

    private int _bucketType;
    /// <summary>
    /// Bucket Types:
    /// <para>
    /// 1 - Standard Bucket<br />
    /// 2 - Monthly expense<br />
    /// 3 - Expense every X Months<br />
    /// 4 - Save X until Y date
    /// </para>
    /// </summary>
    [Required]
    public int BucketType
    {
        get => _bucketType;
        set
        {
            Set(ref _bucketType, value);
            switch (value)
            {
                case 1:
                    BucketTypeXParam = 0;
                    BucketTypeYParam = 0;
                    BucketTypeZParam = DateTime.MinValue;
                    break;
                case 2:
                    BucketTypeXParam = 1;
                    BucketTypeZParam = DateTime.MinValue;
                    break;
                case 3:
                    break;
                case 4:
                    BucketTypeXParam = 0;
                    break;
            }
        }
    }

    private int _bucketTypeXParam;
    /// <summary>
    /// Parameter for number of months. For BucketType:
    /// <para>
    /// 1 - 0<br />
    /// 2 - 1<br />
    /// 3 - int Months<br />
    /// 4 - 0
    /// </para>
    /// </summary>
    public int BucketTypeXParam
    {
        get => _bucketTypeXParam;
        set => Set(ref _bucketTypeXParam, value);
    }

    private decimal _bucketTypeYParam;
    /// <summary>
    /// Parameter for an Amount value. For BucketType:
    /// <para>
    /// 1 - 0<br />
    /// 2-3 - decimal Amount<br />
    /// 4 - decimal Amount
    /// </para>
    /// </summary>
    [Column(TypeName = "decimal(65, 2)")]
    public decimal BucketTypeYParam
    {
        get => _bucketTypeYParam;
        set => Set(ref _bucketTypeYParam, value);
    }

    private DateTime _bucketTypeZParam;
    /// <summary>
    /// Parameter for target date value. For BucketType:
    /// <para>
    /// 1-2 - string.Empty<br />
    /// 3 - DateTime First target date<br />
    /// 4 - DateTime Target date
    /// </para>
    /// </summary>
    public DateTime BucketTypeZParam
    {
        get => _bucketTypeZParam;
        set => Set(ref _bucketTypeZParam, value);
    }

    private string _notes;
    public string Notes
    {
        get => _notes;
        set => Set(ref _notes, value);
    }

    private DateTime _validFrom;
    [Required]
    public DateTime ValidFrom
    {
        get => _validFrom;
        set => Set(ref _validFrom, value);
    }
}
