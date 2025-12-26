namespace Domain.Entities.Suppliers;

public sealed class Supplier : EntityBase
{
    public string TradeName { get; set; } = string.Empty;

    public string CorporateName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string Cnpj { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Address Address { get; set; } = null!;

    public ICollection<SupplierProduct> SupplierProducts { get; set; } = [];

    public long SupplierCategoryId { get; set; }
    public SupplierCategory SupplierCategory { get; set; } = null!;
}