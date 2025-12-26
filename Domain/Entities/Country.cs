namespace Domain.Entities;

public sealed class Country : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}