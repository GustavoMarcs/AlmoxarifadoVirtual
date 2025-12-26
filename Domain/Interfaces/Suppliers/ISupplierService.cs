using Domain.Entities.Suppliers;
using Domain.Enums;

namespace Domain.Interfaces.Suppliers;

public interface ISupplierService : IService<Supplier>
{
    Task<PagedResult<Supplier>> GetSuppliersByCategoryAsync(
        QueryOptions request,
        long? categoryId = null,
        IsActiveOptions isActive = IsActiveOptions.All,
        CancellationToken cancellationToken = default);
}