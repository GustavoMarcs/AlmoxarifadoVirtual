using System.Linq.Expressions;
using Application.Extensions;
using Domain.Abstractions;
using Domain.Entities.Products;
using Domain.Interfaces;
using Domain.Interfaces.Products;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Products;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IDbContextFactory<AlmoxarifadoVirtualContext> _contextFactory;

    public ProductCategoryService(IDbContextFactory<AlmoxarifadoVirtualContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ProductCategory?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.ProductCategories
            .AsNoTracking()
            .Include(pc => pc.SupplierProducts)
            .ThenInclude(pc => pc.ProductCategory)
            .AsQueryable();

        return await query.FirstOrDefaultAsync(c => c.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<PagedResult<ProductCategory>> GetAllAsync(
        QueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (options is null)
        {
            var categories = await context.ProductCategories
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PagedResult<ProductCategory>(categories, categories.Count, 1, categories.Count);
        }

        // Query base sem Include para contagem
        IQueryable<ProductCategory> baseQuery = context.ProductCategories
            .AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                s => s.Name.ToLower().Contains(options.SearchTerm!.ToLower()));

        // Contar total antes de aplicar ordenação e paginação
        var totalCount = await baseQuery.CountAsync(cancellationToken: cancellationToken);

        // Query para dados com Include e ordenação
        IQueryable<ProductCategory> dataQuery = context.ProductCategories
            .Include(pc => pc.SupplierProducts)
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                s => s.Name.ToLower().Contains(options.SearchTerm!.ToLower()));

        // Aplicar ordenação
        if (!string.IsNullOrEmpty(options.SortOrder))
        {
            if (options.SortOrder == "desc")
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


        return new PagedResult<ProductCategory>(items, totalCount, options.Page, options.PageSize);
    }

    public async Task<Result> AddAsync(ProductCategory entity, CancellationToken cancellationToken = default)
    {

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Verificar se já existe uma categoria com o mesmo nome
        if (await context.ProductCategories.AnyAsync(pc => pc.Name == entity.Name, cancellationToken: cancellationToken))
        {
            return Result.Failure(ErrorMessageBase.AlreadyExists(entityName: entity.Name,
                entityType: "categoria de produto", isFemaleName: true));
        }

        context.ProductCategories.Add(entity);
        await context.SaveChangesAsync(cancellationToken);


        return Result.Success();
    }

    public async Task<Result> UpdateAsync(ProductCategory entity, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Verificar se já existe outra categoria com o mesmo nome (excluindo a atual)
        if (await context.ProductCategories.AnyAsync(pc => pc.Name == entity.Name && pc.Id != entity.Id,
            cancellationToken: cancellationToken))
        {
            return Result.Failure(ErrorMessageBase.AlreadyExists(entityName: entity.Name,
                entityType: "categoria de produto", isFemaleName: true));
        }

        entity.UpdatedAt = DateTime.UtcNow;

        context.ProductCategories.Update(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var productCategory = await context.ProductCategories
            .Include(pc => pc.SupplierProducts)
            .FirstOrDefaultAsync(pc => pc.Id == id, cancellationToken: cancellationToken);

        if (productCategory is null)
        {
            return Result.Failure(ErrorMessageBase.NotExists("categoria"));
        }

        if (productCategory.SupplierProducts.Any())
        {
            return Result.Failure(ErrorMessageBase.CannotDelete("categoria de produto", "existem produtos de fornecedor associados"));
        }

        context.ProductCategories.Remove(productCategory);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<IReadOnlyCollection<ProductCategory>> GetAllWithoutPaginationAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ProductCategories.AsNoTracking().ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<int> CountEntityAsync(Expression<Func<ProductCategory, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return predicate is not null ?
            await context.ProductCategories.CountAsync(predicate, cancellationToken) :
            await context.ProductCategories.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductCategory>> GetAllBySupplierAsync(
        long supplierId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.SupplierProducts
            .Where(sp => sp.SupplierId == supplierId)
            .Select(sp => sp.ProductCategory)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static Expression<Func<ProductCategory, object>> GetSortProperty(QueryOptions request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => category => category.Name,
            "productscount" => category => category.SupplierProducts.Count,
            _ => category => category.CreatedAt
        };
    }
}