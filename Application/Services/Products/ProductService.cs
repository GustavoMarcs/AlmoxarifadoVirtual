using System.Linq.Expressions;
using Application.Extensions;
using Domain.Abstractions;
using Domain.Entities.Products;
using Domain.Filters;
using Domain.Interfaces;
using Domain.Interfaces.Products;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Products;

public class ProductService : IProductService
{
    private readonly IDbContextFactory<AlmoxarifadoVirtualContext> _contextFactory;
    private readonly ProductPriceHistoryService _productPriceHistoryService;

    public ProductService(
        IDbContextFactory<AlmoxarifadoVirtualContext> contextFactory,
        ProductPriceHistoryService productPriceHistoryService)
    {
        _contextFactory = contextFactory;
        _productPriceHistoryService = productPriceHistoryService;
    }

    public async Task<Product?> GetByIdAsync(long id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Products
            .AsNoTracking()
            .Include(p => p.ProductPriceHistories)
            .Include(p => p.SupplierProduct)
            .ThenInclude(p => p.ProductCategory)
            .Include(p => p.ProductLocation)
            .AsQueryable();

        return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<PagedResult<Product>> GetAllAsync(
        QueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (options is null)
        {
            var products = await context.Products
                .AsNoTracking()
                .Include(p => p.ProductPriceHistories)
                .Include(p => p.SupplierProduct)
                .ThenInclude(p => p.ProductCategory)
                .Include(p => p.ProductLocation)
                .ToListAsync(cancellationToken);

            return new PagedResult<Product>(products, products.Count, 1, products.Count);
        }


        // Query base sem Include para contagem
        IQueryable<Product> baseQuery = context.Products
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                p => p.Name.ToLower().Contains(options.SearchTerm!.ToLower()));

        // Contar total antes de aplicar ordenação e paginação
        var totalCount = await baseQuery.CountAsync(cancellationToken: cancellationToken);

        // Query para dados com Include e ordenação
        IQueryable<Product> dataQuery = context.Products
            .AsNoTracking()
            .Include(p => p.ProductPriceHistories)
            .Include(p => p.SupplierProduct)
            .Include(p => p.ProductLocation)
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                p => p.Name.ToLower().Contains(options.SearchTerm!.ToLower()));

        // Aplicar ordenação
        if (!string.IsNullOrEmpty(options.SortOrder))
        {
            if (options.SortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                dataQuery = dataQuery.OrderByDescending(GetSortProperty(options));
            }
            else
            {
                dataQuery = dataQuery.OrderBy(GetSortProperty(options));
            }
        }

        // Aplicar paginação
        var items = await dataQuery
            .Skip((options.Page - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>(items, totalCount, options.Page, options.PageSize);
    }

    public async Task<Result> AddAsync(Product entity,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.Products
                .AnyAsync(p => p.Name == entity.Name &&
                               p.SupplierProductId == entity.SupplierProductId,
                    cancellationToken: cancellationToken))
        {
            return Result.Failure(ErrorMessageBase.AlreadyExists(entityName: entity.Name, entityType: "produto",
                isFemaleName: false));
        }

        context.Products.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(Product entity,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.Products.AnyAsync(p => p.Name == entity.Name &&
                                                 p.Id != entity.Id &&
                                                 p.SupplierProductId != entity.SupplierProductId,
                cancellationToken: cancellationToken))
        {
            return Result.Failure(ErrorMessageBase.AlreadyExists(entityName: entity.Name, entityType: "produto",
                isFemaleName: false));
        }

        // Caso tenha mudado o preço, vai atualizar o histórico de preços desse produto.
        await _productPriceHistoryService.UpdateProductPriceHistory(entity, context, cancellationToken);

        entity.UpdatedAt = DateTime.UtcNow;

        context.Products.Update(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id,
            cancellationToken: cancellationToken);

        if (product is null)
        {
            return Result.Failure(ErrorMessageBase.NotExists("produto"));
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


    public async Task<PagedResult<Product>> GetAllByFilterAsync(
        QueryOptions options,
        ProductFilter filter,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Query base sem Include para contagem
        IQueryable<Product> baseQuery = context.Products
            .AsNoTracking()
            .Include(p => p.ProductPriceHistories)
            .Include(p => p.SupplierProduct)
            .ThenInclude(sp => sp.ProductCategory)
            .Include(p => p.ProductLocation)
            .WhereIf(filter.CategoryId > 0, p => p.SupplierProduct.ProductCategoryId == filter.CategoryId)
            .WhereIf(filter.SupplierId > 0, p => p.SupplierProduct.SupplierId == filter.SupplierId)
            .WhereIf(filter.LocationId > 0, p => p.ProductLocationId == filter.LocationId)
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                p => p.Name.ToLower().Contains(options.SearchTerm!.ToLower()));

        if (!string.IsNullOrEmpty(options.SortOrder))
        {
            if (options.SortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                baseQuery = baseQuery.OrderByDescending(GetSortProperty(options));
            }
            else
            {
                baseQuery = baseQuery.OrderBy(GetSortProperty(options));
            }
        }

        // Contar total antes de aplicar ordenação e paginação
        var totalCount = await baseQuery.CountAsync(cancellationToken: cancellationToken);

        // Aplicar paginação
        var items = await baseQuery
            .Skip((options.Page - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return new PagedResult<Product>(items, totalCount, options.Page, options.PageSize);
    }

    public async Task<int> CountEntityAsync(Expression<Func<Product, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return predicate is not null
            ? await context.Products.CountAsync(predicate, cancellationToken)
            : await context.Products.CountAsync(cancellationToken);
    }

    private static Expression<Func<Product, object>> GetSortProperty(QueryOptions request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => product => product.Name,
            "amount" => product => product.Amount,
            "category" => product => product.SupplierProduct.ProductCategory.Name,
            "location" => product => product.ProductLocation.Name,
            "price" => product => product.SellingPrice,
            "updatedAt" => product => product.UpdatedAt ?? product.CreatedAt,
            _ => product => product.Name
        };
    }

}