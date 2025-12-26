using Domain.Entities.Products;

namespace Domain.Entities.Suppliers;

public sealed class SupplierProduct : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? Sku { get; set; } = string.Empty;

    public string? Barcode { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = [];

    public long SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public long ProductCategoryId { get; set; }
    public ProductCategory ProductCategory { get; set; } = null!;
}