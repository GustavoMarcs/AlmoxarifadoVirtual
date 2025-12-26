using Domain.Entities.Products;
using Domain.Entities.Tracker;
using Domain.Filters;

namespace Domain.Interfaces.MovementSimulator;

public interface IMovementValidator
{
    bool IsValid(
        Product product,
        Movement movement,
        CancellationToken cancellationToken = default);
}