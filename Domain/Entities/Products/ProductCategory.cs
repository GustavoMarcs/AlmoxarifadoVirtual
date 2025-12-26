using Domain.Entities.Suppliers;

namespace Domain.Entities.Products;

public sealed class ProductCategory : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public ICollection<SupplierProduct> SupplierProducts { get; set; } = [];
}