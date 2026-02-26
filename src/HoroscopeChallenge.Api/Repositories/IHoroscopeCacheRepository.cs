using HoroscopeChallenge.Api.Domain.Entities;

namespace HoroscopeChallenge.Api.Repositories;

public interface IHoroscopeCacheRepository
{
    Task<HoroscopeCache?> GetAsync(string sign, DateOnly date, string lang);
    Task CreateAsync(HoroscopeCache cache);
}
