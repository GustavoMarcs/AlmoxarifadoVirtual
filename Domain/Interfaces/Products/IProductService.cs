using Domain.Entities.Products;
using Domain.Filters;

namespace Domain.Interfaces.Products;

public interface IProductService : IService<Product>
{
    Task<PagedResult<Product>> GetAllByFilterAsync(
        QueryOptions options,
        ProductFilter filter,
        CancellationToken cancellationToken);
}