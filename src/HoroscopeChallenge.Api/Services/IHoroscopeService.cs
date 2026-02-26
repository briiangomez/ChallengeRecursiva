using HoroscopeChallenge.Api.DTOs;

namespace HoroscopeChallenge.Api.Services;

public interface IHoroscopeService
{
    Task<HoroscopeTodayResponse> GetTodayAsync(int userId, string lang);
    Task<MostQueriedSignResponse?> GetMostQueriedSignAsync();
}
