namespace Domain.Interfaces;

public record QueryOptions(
    int Page,
    int PageSize,
    string? SortColumn,
    string? SortOrder,
    string? SearchTerm
);

