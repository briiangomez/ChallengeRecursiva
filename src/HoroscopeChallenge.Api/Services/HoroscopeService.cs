using System.Text;
using System.Text.Json;
using HoroscopeChallenge.Api.Domain.Entities;
using HoroscopeChallenge.Api.Domain.Helpers;
using HoroscopeChallenge.Api.DTOs;
using HoroscopeChallenge.Api.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace HoroscopeChallenge.Api.Services;

public class HoroscopeService : IHoroscopeService
{
    private readonly IUserRepository _users;
    private readonly IHoroscopeCacheRepository _cacheRepo;
    private readonly IHoroscopeQueryHistoryRepository _historyRepo;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _config;

    public HoroscopeService(
        IUserRepository users,
        IHoroscopeCacheRepository cacheRepo,
        IHoroscopeQueryHistoryRepository historyRepo,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IConfiguration config)
    {
        _users             = users;
        _cacheRepo         = cacheRepo;
        _historyRepo       = historyRepo;
        _httpClientFactory = httpClientFactory;
        _memoryCache       = memoryCache;
        _config            = config;
    }

    public async Task<HoroscopeTodayResponse> GetTodayAsync(int userId, string lang)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sign  = ZodiacHelper.GetSign(user.BirthDate);
        var days  = ZodiacHelper.GetDaysToBirthday(user.BirthDate, today);

        var horoscope = await GetHoroscopeTextAsync(sign, today, lang);

        await _historyRepo.CreateAsync(new HoroscopeQueryHistory
        {
            UserId    = userId,
            Sign      = sign,
            Date      = today,
            Lang      = lang,
            CreatedAt = DateTime.UtcNow
        });

        return new HoroscopeTodayResponse(sign, horoscope, days, today);
    }

    public async Task<MostQueriedSignResponse?> GetMostQueriedSignAsync()
    {
        var result = await _historyRepo.GetMostQueriedSignAsync();
        return result is null ? null : new MostQueriedSignResponse(result.Value.Sign, result.Value.Count);
    }

    private async Task<string> GetHoroscopeTextAsync(string sign, DateOnly date, string lang)
    {
        var cacheKey = $"horoscope:{sign}:{date:yyyy-MM-dd}:{lang}";

        if (_memoryCache.TryGetValue(cacheKey, out string? cached) && cached is not null)
            return cached;

        var dbCache = await _cacheRepo.GetAsync(sign, date, lang);
        if (dbCache is not null)
        {
            SetMemoryCache(cacheKey, dbCache.ResponseJson);
            return ExtractHoroscopeText(dbCache.ResponseJson);
        }

        var responseJson = await FetchFromExternalApiAsync(sign, lang);

        await _cacheRepo.CreateAsync(new HoroscopeCache
        {
            Sign         = sign,
            Date         = date,
            Lang         = lang,
            ResponseJson = responseJson,
            CachedAt     = DateTime.UtcNow
        });

        SetMemoryCache(cacheKey, responseJson);

        return ExtractHoroscopeText(responseJson);
    }

    private async Task<string> FetchFromExternalApiAsync(string sign, string lang)
    {
        var client = _httpClientFactory.CreateClient("HoroscopeApi");

        var payload = new { sign, lang };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync("/", content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private static string ExtractHoroscopeText(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("horoscope", out var prop))
                return prop.GetString() ?? json;
        }
        catch { }

        return json;
    }

    private void SetMemoryCache(string key, string value)
    {
        var midnight = DateTime.UtcNow.Date.AddDays(1);
        _memoryCache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = midnight
        });
    }
}
