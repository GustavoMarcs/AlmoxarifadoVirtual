using Domain.Entities.Products;

namespace Domain.Interfaces.Products;

public interface IProductCategoryService : IService<ProductCategory>
{
    Task<IEnumerable<ProductCategory>> GetAllBySupplierAsync(long supplierId, CancellationToken cancellationToken = default);
}
