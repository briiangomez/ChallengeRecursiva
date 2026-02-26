using HoroscopeChallenge.Api.Data;
using HoroscopeChallenge.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoroscopeChallenge.Api.Repositories;

public class HoroscopeCacheRepository : IHoroscopeCacheRepository
{
    private readonly AppDbContext _db;

    public HoroscopeCacheRepository(AppDbContext db) => _db = db;

    public Task<HoroscopeCache?> GetAsync(string sign, DateOnly date, string lang)
        => _db.HoroscopeCache
              .FirstOrDefaultAsync(h => h.Sign == sign && h.Date == date && h.Lang == lang);

    public async Task CreateAsync(HoroscopeCache cache)
    {
        _db.HoroscopeCache.Add(cache);
        await _db.SaveChangesAsync();
    }
}
