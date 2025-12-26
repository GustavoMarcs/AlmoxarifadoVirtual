namespace Domain.Interfaces;

public class PagedResult<T>(IEnumerable<T> items, int totalCount, int page, int pageSize)
{
    public IEnumerable<T> Items { get; } = items;
    public int TotalCount { get; } = totalCount;
    public int Page { get; init; } = page;
    public int PageSize { get; } = pageSize;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}