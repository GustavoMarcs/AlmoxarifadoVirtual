using Domain.Enums;

namespace Domain.Filters;

public sealed class MovementFilter(
    long supplierId = 0,
    long locationId = 0,
    DateFilterType dateFilter = DateFilterType.All)
{
    public long SupplierId { get; set; } = supplierId;
    public long LocationId { get; set; } = locationId;
    public DateFilterType DateFilter { get; set; } = dateFilter;
}
