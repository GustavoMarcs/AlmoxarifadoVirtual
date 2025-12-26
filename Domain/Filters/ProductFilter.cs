namespace Domain.Filters;

public sealed class ProductFilter(long categoryId = 0, long supplierId = 0, long locationId = 0)
{
    public long CategoryId { get; set; } = categoryId;
    public long SupplierId { get; set; } = supplierId;
    public long LocationId { get; set; } = locationId;
}