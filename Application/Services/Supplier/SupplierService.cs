using System.Linq.Expressions;
using Application.Extensions;
using Domain.Abstractions;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Interfaces.Suppliers;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Supplier;

public class SupplierService : ISupplierService
{
    private readonly IDbContextFactory<AlmoxarifadoVirtualContext> _contextFactory;

    public SupplierService(IDbContextFactory<AlmoxarifadoVirtualContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Domain.Entities.Suppliers.Supplier?> GetByIdAsync(long id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Suppliers
            .AsNoTracking()
            .Include(s => s.SupplierProducts)
            .Include(s => s.SupplierCategory)
            .AsQueryable();

        return await query.FirstOrDefaultAsync(s => s.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<PagedResult<Domain.Entities.Suppliers.Supplier>> GetAllAsync(
        QueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (options is null)
        {
            var suppliers = await context.Suppliers.AsNoTracking().ToListAsync(cancellationToken);
            return new PagedResult<Domain.Entities.Suppliers.Supplier>(suppliers, suppliers.Count, 1, suppliers.Count);
        }

        // Query base sem Include para contagem
        IQueryable<Domain.Entities.Suppliers.Supplier> baseQuery = context.Suppliers;

        // Contar total antes de aplicar ordenação e paginação
        var totalCount = await baseQuery.CountAsync(cancellationToken: cancellationToken);

        // Query para dados com Include e ordenação
        IQueryable<Domain.Entities.Suppliers.Supplier> dataQuery = context.Suppliers
            .AsNoTracking()
            .Include(s => s.SupplierProducts)
            .Include(s => s.SupplierCategory)
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                sp => sp.TradeName.ToLower().Contains(options.SearchTerm!.ToLower()));

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

        return new PagedResult<Domain.Entities.Suppliers.Supplier>(items, totalCount, options.Page, options.PageSize);
    }

    public async Task<Result> AddAsync(Domain.Entities.Suppliers.Supplier entity, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Verificar se já existe um fornecedor com o mesmo CNPJ
        if (!string.IsNullOrWhiteSpace(entity.Cnpj) &&
            await context.Suppliers.AnyAsync(s => s.Cnpj == entity.Cnpj, cancellationToken: cancellationToken))
        {
            return Result.Failure(SupplierErrors.CnpjAlreadyExists);
        }

        context.Suppliers.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(Domain.Entities.Suppliers.Supplier entity, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Verificar se já existe outro fornecedor com o mesmo nome (excluindo o atual)
        if (await context.Suppliers.AnyAsync(s => s.TradeName == entity.TradeName && s.Id != entity.Id,
                cancellationToken: cancellationToken))
        {
            return Result.Failure(ErrorMessageBase.AlreadyExists(entityName: entity.TradeName, 
                entityType: "fornecedor", isFemaleName: false));
        }

        // Verificar se já existe outro fornecedor com o mesmo CNPJ (excluindo o atual)
        if (!string.IsNullOrWhiteSpace(entity.Cnpj) &&
            await context.Suppliers.AnyAsync(s => s.Cnpj == entity.Cnpj && s.Id != entity.Id, 
                cancellationToken: cancellationToken))
        {
            return Result.Failure(SupplierErrors.CnpjAlreadyExists);
        }

        entity.UpdatedAt = DateTime.UtcNow;

        context.Suppliers.Update(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var supplier = await context.Suppliers
            .Include(s => s.SupplierProducts)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken: cancellationToken);

        if (supplier is null)
        {
            return Result.Failure(ErrorMessageBase.NotExists("fornecedor"));
        }

        if (supplier.SupplierProducts.Any())
        {
            return Result.Failure(ErrorMessageBase.CannotDelete("fornecedor", "existem produtos de fornecedor associados"));
        }

        context.Suppliers.Remove(supplier);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<PagedResult<Domain.Entities.Suppliers.Supplier>> GetSuppliersByCategoryAsync(
        QueryOptions request,
        long? categoryId = null,
        IsActiveOptions isActive = IsActiveOptions.All,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        IQueryable<Domain.Entities.Suppliers.Supplier> baseQuery = context.Suppliers
            .AsNoTracking();

        IQueryable<Domain.Entities.Suppliers.Supplier> dataQuery = baseQuery
            .Include(s => s.SupplierProducts)
            .Include(s => s.SupplierCategory)
            .WhereIf(!string.IsNullOrWhiteSpace(request.SearchTerm),
                sp => sp.TradeName.ToLower().Contains(request.SearchTerm!.ToLower()))
            .WhereIf(categoryId is not null && categoryId != 0, ct => ct.SupplierCategoryId == categoryId)
            .WhereIf(isActive == IsActiveOptions.Active, s => s.IsActive)
            .WhereIf(isActive == IsActiveOptions.Inactive, s => !s.IsActive);

        // Aplicar ordenação
        if (!string.IsNullOrEmpty(request.SortOrder))
        {
            if (request.SortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                dataQuery = dataQuery.OrderByDescending(GetSortProperty(request));
            }
            else
            {
                dataQuery = dataQuery.OrderBy(GetSortProperty(request));
            }
        }

        // Contar total antes de aplicar ordenação e paginação
        var totalCount = await dataQuery.CountAsync(cancellationToken: cancellationToken);

        var items = await dataQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return new PagedResult<Domain.Entities.Suppliers.Supplier>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<IReadOnlyCollection<Domain.Entities.Suppliers.Supplier>> GetAllWithoutPaginationAsync(CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Suppliers
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountEntityAsync(Expression<Func<Domain.Entities.Suppliers.Supplier, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return predicate is not null ?
            await context.Suppliers.CountAsync(predicate, cancellationToken) :
            await context.Suppliers.CountAsync(cancellationToken);
    }

    private static Expression<Func<Domain.Entities.Suppliers.Supplier, object>> GetSortProperty(QueryOptions request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => supplier => supplier.TradeName,
            "cnpj" => supplier => supplier.Cnpj,
            "category" => supplier => supplier.SupplierCategory.Name,
            "country" => supplier => supplier.Address.Country.Name,
            "status" => supplier => supplier.IsActive,
            "productscount" => supplier => supplier.SupplierProducts.Count,
            "totalvalue" => supplier => supplier.SupplierProducts.Sum(s => s.Price),
            _ => supplier => supplier.CreatedAt
        };
    }
}

public static class SupplierErrors
{
    public static readonly Error CnpjAlreadyExists =
        new("Supplier.CnpjAlreadyExists", "Já existe um fornecedor com esse CNPJ.");
}

