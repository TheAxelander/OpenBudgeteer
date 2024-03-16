namespace OpenBudgeteer.Core.Data.Contracts.Services;

public record BucketFigures()
{
    public decimal? Balance { get; set; }
    public decimal Input { get; set; }
    public decimal Output { get; set; }
}