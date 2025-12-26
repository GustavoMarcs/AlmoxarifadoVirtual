using System.Linq.Expressions;
using Application.Extensions;
using Domain.Abstractions;
using Domain.Entities.Tracker;
using Domain.Enums;
using Domain.Filters;
using Domain.Interfaces;
using Domain.Interfaces.MovementSimulator;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.MovementSimulator;

public class MovementService : IMovementService
{
    private readonly IDbContextFactory<AlmoxarifadoVirtualContext> _context;

    public MovementService(
        IDbContextFactory<AlmoxarifadoVirtualContext> context)
    {
        _context = context;
    }

    public async Task<Result> RegisterMovementAsync(
        Movement movement,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _context.CreateDbContextAsync(cancellationToken);

        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == movement.ProductId, cancellationToken);

        if (product is null)
            return Result.Failure(
                ErrorMessageBase.NotExists($"Produto com Id {movement.ProductId}")
            );

        // Atualiza estoque
        int sign = movement.Type == MovementType.In ? 1 : -1;
        product.Amount += sign * movement.Quantity;

        // Persiste alterações
        context.Movements.Add(movement);
        context.Products.Update(product);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task SimulateMovementsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _context.CreateDbContextAsync(cancellationToken);

        int currentMonthNumber = DateTime.UtcNow.Month;
        var products = await context.Products.ToListAsync(cancellationToken);

        if (!products.Any())
        {
            return;
        }

        List<Movement> movements = [];

        for (var i = 0; i < currentMonthNumber; i++)
        {
            for (var j = 0; j < Random.Shared.Next(minValue: 100, maxValue: 200); j++)
            {
                var product = products[Random.Shared.Next(0, products.Count - 1)];

                // Determinar o tipo de movimento e a quantidade
                var type = Random.Shared.Next(0, 2) == 0 ? MovementType.In : MovementType.Out;
                var quantity = 0;

                if (type == MovementType.In)
                {
                    var spaceLeft = product.MaximalQuantity - product.Amount;

                    if (spaceLeft > 0)
                    {
                        quantity = Random.Shared.Next(1, spaceLeft + 1);
                    }
                }
                else // MovementType.Out
                {
                    if (product.Amount > 0)
                    {
                        quantity = Random.Shared.Next(1, product.Amount + 1);
                    }
                }

                var movement = new Movement
                {
                    CreatedAt = new DateTime(2025, i + 1, Random.Shared.Next(1, 29)),
                    ProductId = product.Id,
                    Type = type,
                    Quantity = quantity
                };

                movements.Add(movement);
            }
        }

        // Salva os movimentos restantes no final
        if (movements.Count > 0)
        {
            await context.Movements.AddRangeAsync(movements, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<PagedResult<Movement>> GetAllAsync(
        QueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _context.CreateDbContextAsync(cancellationToken);

        if (options is null)
        {
            var movements = await context.Movements
                .AsNoTracking()
                .Include(m => m.Product)
                .ThenInclude(p => p.SupplierProduct)
                .ThenInclude(sp => sp.ProductCategory)
                .Include(m => m.Product)
                .ThenInclude(p => p.ProductLocation)
                .ToListAsync(cancellationToken);

            return new PagedResult<Movement>(movements, movements.Count, 1, movements.Count);
        }

        // Query base sem Include para contagem
        IQueryable<Movement> baseQuery = context.Movements
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                m => m.Product.Name.ToLower().Contains(options.SearchTerm!.ToLower()) ||
                     m.Product.SupplierProduct.Sku!.ToLower().Contains(options.SearchTerm!.ToLower()));

        // Contar total antes de aplicar ordenação e paginação
        var totalCount = await baseQuery.CountAsync(cancellationToken: cancellationToken);

        // Query para dados com Include e ordenação
        IQueryable<Movement> dataQuery = context.Movements
            .AsNoTracking()
            .Include(m => m.Product)
            .ThenInclude(p => p.SupplierProduct)
            .ThenInclude(sp => sp.ProductCategory)
            .Include(m => m.Product)
            .ThenInclude(p => p.ProductLocation)
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                m => m.Product.Name.ToLower().Contains(options.SearchTerm!.ToLower()) ||
                     m.Product.SupplierProduct.Sku!.ToLower().Contains(options.SearchTerm!.ToLower()));

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

        return new PagedResult<Movement>(items, totalCount, options.Page, options.PageSize);
    }

    public async Task<PagedResult<Movement>> GetAllByFilterAsync(
        QueryOptions options,
        MovementFilter filter,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _context.CreateDbContextAsync(cancellationToken);

        // Query base sem Include para contagem
        IQueryable<Movement> baseQuery = context.Movements
            .AsNoTracking()
            .Include(m => m.Product)
            .ThenInclude(m => m.ProductLocation)
            .Include(m => m.Product)
            .ThenInclude(m => m.SupplierProduct)
            .ThenInclude(m => m.Supplier)
            .WhereIf(filter.SupplierId > 0, m => m.Product.SupplierProduct.SupplierId == filter.SupplierId)
            .WhereIf(filter.LocationId > 0, m => m.Product.ProductLocationId == filter.LocationId)
            .WhereIf(!string.IsNullOrWhiteSpace(options.SearchTerm),
                m => m.Product.Name.ToLower().Contains(options.SearchTerm!.ToLower()))
            .WhereIf(filter.DateFilter != DateFilterType.All, m => GetDateFilterCondition(m.CreatedAt, filter.DateFilter));

        // Contar total antes de aplicar ordenação e paginação
        var totalCount = await baseQuery.CountAsync(cancellationToken: cancellationToken);

        // Aplicar ordenação
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

        // Aplicar paginação
        var items = await baseQuery
            .Skip((options.Page - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Movement>(items, totalCount, options.Page, options.PageSize);
    }

    private static bool GetDateFilterCondition(DateTime createdAt, DateFilterType dateFilter)
    {
        var now = DateTime.UtcNow;
        
        return dateFilter switch
        {
            DateFilterType.ThisMonth => createdAt.Month == now.Month && createdAt.Year == now.Year,
            DateFilterType.Last3Months => createdAt >= now.AddMonths(-3),
            DateFilterType.Last6Months => createdAt >= now.AddMonths(-6),
            _ => true
        };
    }

    private static Expression<Func<Movement, object>> GetSortProperty(QueryOptions request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "product" => movement => movement.Product.Name,
            "type" => movement => movement.Type,
            "quantity" => movement => movement.Quantity,
            "createdAt" => movement => movement.CreatedAt,
            "sku" => movement => movement.Product.SupplierProduct.Sku!,
            "location" => movement => movement.Product.ProductLocation.Name,
            _ => movement => movement.CreatedAt
        };
    }
}