using Domain.Entities.Products;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Products;

public class ProductPriceHistoryService
{
    private readonly IDbContextFactory<AlmoxarifadoVirtualContext> _contextFactory;

    public ProductPriceHistoryService(IDbContextFactory<AlmoxarifadoVirtualContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Atualiza o histórico de preços de um produto. 
    /// Cria um novo registro em <see cref="ProductPriceHistory"/> caso o preço de venda tenha sido alterado.
    /// </summary>
    /// <param name="newProduct">O produto com o preço atualizado que será comparado ao registro atual.</param>
    /// <param name="context"></param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    /// <returns>
    /// Uma <see cref="Task"/> representando a operação assíncrona. 
    /// Se o preço não tiver sido alterado, não será criado nenhum registro.
    /// </returns>
    /// 
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public async Task UpdateProductPriceHistory(
        Product newProduct,
        AlmoxarifadoVirtualContext context,
        CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == newProduct.Id, cancellationToken: cancellationToken);

        if (product is not null)
        {
            if (product.SellingPrice == newProduct.SellingPrice)
            {
                return;
            }

            var productToAdd = new ProductPriceHistory
            {
                OldPrice = product.SellingPrice,
                NewPrice = newProduct.SellingPrice,
                UpdatedPriceAt = DateTime.UtcNow,
                ProductId = product.Id
            };

            context.ProductPriceHistories.Add(productToAdd);
        }
    }
}
