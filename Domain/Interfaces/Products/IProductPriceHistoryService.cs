using Domain.Entities.Products;

namespace Domain.Interfaces.Products;

public interface IProductPriceHistoryService
{
    Task UpdateProductPriceHistory(
        Product newProduct,
        CancellationToken cancellationToken = default);
}
