using Domain.Abstractions;
using Domain.Entities.Tracker;
using Domain.Filters;
using Domain.Interfaces;

namespace Domain.Interfaces.MovementSimulator;

public interface IMovementService
{
    Task<Result> RegisterMovementAsync(
        Movement movement,
        CancellationToken cancellationToken = default);
    
    Task SimulateMovementsAsync(CancellationToken cancellationToken = default);
    
    Task<PagedResult<Movement>> GetAllAsync(
        QueryOptions? options = null,
        CancellationToken cancellationToken = default);
    
    Task<PagedResult<Movement>> GetAllByFilterAsync(
        QueryOptions options,
        MovementFilter filter,
        CancellationToken cancellationToken = default);
}