using Domain.Entities.Products;

namespace Domain.Entities;

public class DepartmentLocation : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}