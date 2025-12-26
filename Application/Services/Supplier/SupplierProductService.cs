using System.Linq.Expressions;
using Application.Extensions;
using Domain.Abstractions;
using Domain.Entities.Suppliers;
using Domain.Filters;
using Domain.Interfaces;
using Domain.Interfaces.Suppliers;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Supplier;

public sealed class SupplierProductService : ISupplierProductService
{
    private readonly IDbContextFactory<AlmoxarifadoVirtualContext> _contextFactory;

    public SupplierProductService(IDbContextFactory<AlmoxarifadoVirtualContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<SupplierProduct?> GetByIdAsync(long id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context
            .SupplierProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id,
            cancellationToken: cancellationToken);
    }

    public async Task<PagedResult<SupplierProduct>> GetAllAsync(
        QueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (options is null)
        {
            var products = await context.SupplierProducts.AsNoTracking().ToListAsync(cancellationToken);
            return new PagedResult<SupplierProduct>(products, products.Count, 1, products.Count);
        }   

        // Query base sem Include para contagem
        IQueryable<SupplierProduct> baseQuery = context.SupplierProducts
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                sp => sp.Name.ToLower().Contains(options.SearchTerm!.ToLower()));

        // Contar total antes de aplicar ordenação e paginação
        var totalCount = await baseQuery.CountAsync(cancellationToken: cancellationToken);

        // Query para dados com Include e ordenação
        IQueryable<SupplierProduct> dataQuery = context.SupplierProducts
            .AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                sp => sp.Name.ToLower().Contains(options.SearchTerm!.ToLower()))
            .Include(sp => sp.ProductCategory)
            .Include(sp => sp.Supplier);

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

        return new PagedResult<SupplierProduct>(items, totalCount, options.Page, options.PageSize);
    }

    public async Task<Result> AddAsync(SupplierProduct entity,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Verificar se já existe um produto com o mesmo nome para o mesmo fornecedor
        if (await context.SupplierProducts.AnyAsync(sp =>
            sp.Name == entity.Name && sp.SupplierId == entity.SupplierId,
            cancellationToken: cancellationToken))
        {
            return Result.Failure(SupplierProductErrors.NameAlreadyExists);
        }

        context.SupplierProducts.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(SupplierProduct entity,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Verificar se já existe outro produto com o mesmo nome para o mesmo fornecedor (excluindo o atual)
        if (await context.SupplierProducts.AnyAsync(sp =>
            sp.Name == entity.Name && sp.SupplierId == entity.SupplierId && sp.Id != entity.Id,
            cancellationToken: cancellationToken))
        {
            return Result.Failure(SupplierProductErrors.NameAlreadyExists);
        }

        entity.UpdatedAt = DateTime.UtcNow;

        context.SupplierProducts.Update(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var supplierProduct = await context.SupplierProducts
            .Include(sp => sp.Products)
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken: cancellationToken);

        if (supplierProduct is null)
        {
            return Result.Failure(SupplierProductErrors.NotExists);
        }

        if (supplierProduct.Products.Any())
        {
            return Result.Failure(ErrorMessageBase.CannotDelete("produto de fornecedor", "existem produtos associados"));
        }

        context.SupplierProducts.Remove(supplierProduct);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<PagedResult<SupplierProduct>> GetProductByFilter(
        QueryOptions options,
        SupplierProductFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        IQueryable<SupplierProduct> baseQuery = context.SupplierProducts.AsNoTracking();

        IQueryable<SupplierProduct> dataQuery = baseQuery
            .WhereIf(!string.IsNullOrWhiteSpace(filter!.SearchTerm), s => s.Name.Contains(filter.SearchTerm))
            .WhereIf(filter.CategoryId > 0, p => p.ProductCategoryId == filter.CategoryId)
            .WhereIf(filter.SupplierId > 0, p => p.SupplierId == filter.SupplierId)
            .Include(p => p.ProductCategory)
            .Include(p => p.Supplier);

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

        // Contar total antes de aplicar ordenação e paginação
        var totalCount = await dataQuery.CountAsync(cancellationToken: cancellationToken);

        var items = await dataQuery
            .Skip((options.Page - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return new PagedResult<SupplierProduct>(items, totalCount, options.Page, options.PageSize);
    }


    public async Task<IEnumerable<SupplierProduct>> GetForSupplierByFilterUnpaginatedAsync(
        long id,
        SupplierProductFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var baseQuery = context.SupplierProducts
            .AsNoTracking()
            .Include(s => s.ProductCategory)
            .Include(s => s.Supplier)
            .Where(sp => sp.SupplierId == id);

        var validations = baseQuery
            .WhereIf(!string.IsNullOrWhiteSpace(filter!.SearchTerm), s => s.Name.ToLower().Contains(filter.SearchTerm.ToLower()))
            .WhereIf(filter.CategoryId > 0, pc => pc.ProductCategoryId == filter.CategoryId);

        return await validations.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<int> CountEntityAsync(Expression<Func<SupplierProduct, bool>>? predicate = null, 
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return predicate is not null ?
            await context.SupplierProducts.CountAsync(predicate, cancellationToken) :
            await context.SupplierProducts.CountAsync(cancellationToken);
    }
    public async Task<decimal> GetSumOfProductsValue(CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.SupplierProducts.SumAsync(s => s.Price, cancellationToken: cancellationToken);
    }

    public async Task<decimal> GetTotalPriceOfAllProductAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.SupplierProducts.SumAsync(s => s.Price, cancellationToken: cancellationToken);
    }

    private static Expression<Func<SupplierProduct, object>> GetSortProperty(
        QueryOptions request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => product => product.Name,
            "category" => product => product.ProductCategory.Name,
            "supplier" => product => product.Supplier.TradeName,
            "price" => product => product.Price,
            _ => product => product.CreatedAt,
        };
    }
}

public static class SupplierProductErrors
{
    public static readonly Error NameAlreadyExists =
        new("SupplierProduct.NameAlreadyExists", "Já existe um produto com esse nome para este fornecedor.");

    public static readonly Error NotExists =
        new("SupplierProduct.NotExists", "Esse produto não existe.");
}