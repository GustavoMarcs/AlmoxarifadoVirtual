namespace Domain.Filters;

public class SupplierProductFilter(string searchTerm, long? categoryId, long? supplierId)
{
    public string SearchTerm { get; set; } = searchTerm;
    public long? CategoryId { get; set; } = categoryId;
    public long? SupplierId { get; set; } = supplierId;
}

