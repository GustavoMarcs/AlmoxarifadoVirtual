namespace Domain.Entities.Suppliers;

public sealed class SupplierCategory : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Supplier> Suppliers { get; set; } = [];
}