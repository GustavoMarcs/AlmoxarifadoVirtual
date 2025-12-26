using Application.Extensions;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services;

public class DepartmentLocationService : IDepartmentLocationService
{
    private readonly IDbContextFactory<AlmoxarifadoVirtualContext> _contextFactory;

    public DepartmentLocationService(IDbContextFactory<AlmoxarifadoVirtualContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<DepartmentLocation?> GetByIdAsync(long id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.DepartmentLocalizations
            .AsNoTracking()
            .Include(l => l.Products)
            .AsQueryable();

        return await query.FirstOrDefaultAsync(l => l.Id == id, cancellationToken: cancellationToken);
    }


    public async Task<PagedResult<DepartmentLocation>> GetAllAsync(
        QueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (options is null)
        {
            var departments = await context.DepartmentLocalizations.AsNoTracking().ToListAsync(cancellationToken);
            return new PagedResult<DepartmentLocation>(departments, departments.Count, 1, departments.Count);
        }

        IQueryable<DepartmentLocation> baseQuery = context.DepartmentLocalizations
            .AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                l => l.Name.ToLower().Contains(options.SearchTerm!.ToLower()));

        var totalCount = await baseQuery.CountAsync(cancellationToken: cancellationToken);

        // Query para dados com Include e ordenação
        IQueryable<DepartmentLocation> dataQuery = context.DepartmentLocalizations
            .Include(l => l.Products)
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                l => l.Name.ToLower().Contains(options.SearchTerm!.ToLower()));

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

        return new PagedResult<DepartmentLocation>(items, totalCount, options.Page, options.PageSize);
    }

    public async Task<Result> AddAsync(DepartmentLocation entity, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.DepartmentLocalizations.AnyAsync(l => l.Name == entity.Name,
            cancellationToken: cancellationToken))
        {
            return Result.Failure(ErrorMessageBase.AlreadyExists(entityName: entity.Name, entityType: "localização", isFemaleName: true));
        }

        context.DepartmentLocalizations.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(DepartmentLocation entity, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Verificar se já existe outra localização com o mesmo nome (excluindo a atual)
        if (await context.DepartmentLocalizations.AnyAsync(l => l.Name == entity.Name && l.Id != entity.Id,
            cancellationToken: cancellationToken))
        {
            return Result.Failure(ErrorMessageBase.AlreadyExists(entityName: entity.Name, entityType: "localização", isFemaleName: true));
        }

        entity.UpdatedAt = DateTime.UtcNow;

        context.DepartmentLocalizations.Update(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var departmentLocation = await context.DepartmentLocalizations
            .Include(dl => dl.Products)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken: cancellationToken);

        if (departmentLocation is null)
        {
            return Result.Failure(ErrorMessageBase.NotExists("localização"));
        }

        if (departmentLocation.Products.Any())
        {
            return Result.Failure(ErrorMessageBase.CannotDelete("localização", "existem produtos associados"));
        }

        context.DepartmentLocalizations.Remove(departmentLocation);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<int> CountEntityAsync(Expression<Func<DepartmentLocation, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return predicate is not null ?
            // Caso exista um filtro, contar apenas os que atendem ao filtro
            await context.DepartmentLocalizations.CountAsync(predicate, cancellationToken) :
            await context.DepartmentLocalizations.CountAsync(cancellationToken);
    }


    private static Expression<Func<DepartmentLocation, object>> GetSortProperty(QueryOptions request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => location => location.Name,
            "description" => location => !string.IsNullOrEmpty(location.Description)
                ? location.Description : string.Empty,
            "status" => location => location.IsActive,
            "capacity" => location => location.Capacity,
            "productscount" => location => location.Products.Count,
            _ => location => location.CreatedAt,
        };
    }
}

