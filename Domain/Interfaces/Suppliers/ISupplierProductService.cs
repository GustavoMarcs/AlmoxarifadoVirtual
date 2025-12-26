using Domain.Entities.Suppliers;
using Domain.Filters;

namespace Domain.Interfaces.Suppliers;

public interface ISupplierProductService : IService<SupplierProduct>
{
    Task<IEnumerable<SupplierProduct>> GetForSupplierByFilterUnpaginatedAsync(
        long id,
        SupplierProductFilter? filter = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<SupplierProduct>> GetProductByFilter(
        QueryOptions options,
        SupplierProductFilter? filter = null,
        CancellationToken cancellationToken = default);

    Task<decimal> GetTotalPriceOfAllProductAsync(CancellationToken cancellationToken = default);
}