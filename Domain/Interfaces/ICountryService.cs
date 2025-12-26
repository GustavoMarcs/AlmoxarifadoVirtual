using Domain.Entities;

namespace Domain.Interfaces;

public interface ICountryService
{
    Task<IReadOnlyCollection<Country>> GetAllCountriesAsync();
    Task ImportAllCountriesAsync(CancellationToken cancellationToken = default);
}
