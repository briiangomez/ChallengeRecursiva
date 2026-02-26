namespace HoroscopeChallenge.Api.Repositories;

public interface IHoroscopeQueryHistoryRepository
{
    Task CreateAsync(Domain.Entities.HoroscopeQueryHistory history);
    Task<(string Sign, int Count)?> GetMostQueriedSignAsync();
}
