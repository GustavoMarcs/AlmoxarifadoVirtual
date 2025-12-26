namespace Domain.Entities;

public sealed class Address
{
    public string StreetAdress { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public long CountryId { get; set; }

    public Country Country { get; set; } = null!;
}
