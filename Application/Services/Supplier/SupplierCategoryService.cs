using System.Linq.Expressions;
using Application.Extensions;
using Domain.Abstractions;
using Domain.Entities.Suppliers;
using Domain.Interfaces;
using Domain.Interfaces.Suppliers;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Supplier;

public class SupplierCategoryService : ISupplierCategoryService
{
    private readonly IDbContextFactory<AlmoxarifadoVirtualContext> _contextFactory;

    public SupplierCategoryService(IDbContextFactory<AlmoxarifadoVirtualContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<SupplierCategory?> GetByIdAsync(long id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.SupplierCategories
            .AsNoTracking()
            .Include(sc => sc.Suppliers)
            .AsQueryable();

        return await query.FirstOrDefaultAsync(c => c.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<PagedResult<SupplierCategory>> GetAllAsync(
        QueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (options is null)
        {
            var categories = await context.SupplierCategories.AsNoTracking().ToListAsync(cancellationToken);
            return new PagedResult<SupplierCategory>(categories, categories.Count, 1, categories.Count);
        }

        // Query base sem Include para contagem
        IQueryable<SupplierCategory> baseQuery = context.SupplierCategories
            .AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                sc => sc.Name.ToLower().Contains(options.SearchTerm!.ToLower()));

        // Contar total antes de aplicar ordenação e paginação
        var totalCount = await baseQuery.CountAsync(cancellationToken: cancellationToken);

        // Query para dados com Include e ordenação
        IQueryable<SupplierCategory> dataQuery = context.SupplierCategories

            .Include(sc => sc.Suppliers)
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                sc => sc.Name.ToLower().Contains(options.SearchTerm!.ToLower()));

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

        return new PagedResult<SupplierCategory>(items, totalCount, options.Page, options.PageSize);
    }

    public async Task<Result> AddAsync(SupplierCategory entity, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.SupplierCategories.AnyAsync(sc => sc.Name == entity.Name,
                cancellationToken: cancellationToken))
        {
            return Result.Failure(ErrorMessageBase.AlreadyExists(entityName: entity.Name,
                entityType: "categoria de fornecedor", isFemaleName: true));
        }

        context.SupplierCategories.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(SupplierCategory entity, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Verificar se já existe outra categoria com o mesmo nome (excluindo a atual)
        if (await context.SupplierCategories.AnyAsync(sc => sc.Name == entity.Name && sc.Id != entity.Id,
                cancellationToken: cancellationToken))
        {
            return Result.Failure(ErrorMessageBase
                .AlreadyExists(entityName: entity.Name,
                entityType: "categoria de fornecedor",
                isFemaleName: true));
        }

        entity.UpdatedAt = DateTime.UtcNow;

        context.SupplierCategories.Update(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var supplierCategory = await context.SupplierCategories
            .Include(sc => sc.Suppliers)
            .FirstOrDefaultAsync(sc => sc.Id == id, cancellationToken: cancellationToken);

        if (supplierCategory is null)
        {
            return Result.Failure(ErrorMessageBase.NotExists("categoria"));
        }

        if (supplierCategory.Suppliers.Any())
        {
            return Result.Failure(ErrorMessageBase.CannotDelete("categoria de fornecedor", "existem fornecedores associados"));
        }

        context.SupplierCategories.Remove(supplierCategory);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<IReadOnlyCollection<SupplierCategory>> GetAllWithoutPagination(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.SupplierCategories.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<int> CountEntityAsync(Expression<Func<SupplierCategory, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return predicate is not null ?
            await context.SupplierCategories.CountAsync(predicate, cancellationToken) :
            await context.SupplierCategories.CountAsync(cancellationToken);
    }


    private static Expression<Func<SupplierCategory, object>> GetSortProperty(QueryOptions request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => category => category.Name,
            "supplierscount" => category => category.Suppliers.Count,
            _ => category => category.CreatedAt,
        };
    }
}
