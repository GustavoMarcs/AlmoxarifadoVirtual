using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Services;

public sealed class CountryService : ICountryService
{
    private readonly IDbContextFactory<AlmoxarifadoVirtualContext> _contextFactory;
    private readonly HttpClient _httpClient;

    public CountryService(HttpClient httpClient, IDbContextFactory<AlmoxarifadoVirtualContext> contextFactory)
    {
        _httpClient = httpClient;
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Get all countries from the database as a read-only collection.
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<Country>> GetAllCountriesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Countries.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Import all countries from the external API and save them to the database. REMOVE ALL MANNUALY COUNTRIES BEFORE IMPORTING.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task ImportAllCountriesAsync(CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.Countries.AnyAsync(cancellationToken: cancellationToken)) // Returns if countries table has any data.
        {
            return;
        }

        using var response = await _httpClient.GetAsync("https://restcountries.com/v3.1/all?fields=name,cca2,translations", cancellationToken);
        response.EnsureSuccessStatusCode();

        var countriesApiResponse = await JsonSerializer
            .DeserializeAsync<List<CountryApiResponse>>(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

        if (countriesApiResponse is null || countriesApiResponse.Count == 0)
        {
            throw new InvalidOperationException("A resposta da API de países está vazia ou nula.");
        }

        var countries = countriesApiResponse.Select(c => new Country
        {
            Code = c.Cca2,
            Name = c.Translations.Por.Common
        }).ToList();

        var countriesWithoutDuplicates = countries.DistinctBy(x => x.Name).DistinctBy(x => x.Code);

        await context.Countries.AddRangeAsync(countriesWithoutDuplicates, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}


public record CountryApiResponse
{
    [JsonPropertyName("cca2")]
    public required string Cca2 { get; init; }

    [JsonPropertyName("translations")]
    public required Translation Translations { get; init; }

    // Nome final em português
    [JsonIgnore]
    public string Name => Translations.Por.Common;
}

public record Translation
{
    [JsonPropertyName("por")]
    public required CountryName Por { get; init; }
}

public record CountryName
{
    [JsonPropertyName("common")]
    public required string Common { get; init; }
}
