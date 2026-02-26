using HoroscopeChallenge.Api.Data;
using HoroscopeChallenge.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoroscopeChallenge.Api.Repositories;

public class HoroscopeQueryHistoryRepository : IHoroscopeQueryHistoryRepository
{
    private readonly AppDbContext _db;

    public HoroscopeQueryHistoryRepository(AppDbContext db) => _db = db;

    public async Task CreateAsync(HoroscopeQueryHistory history)
    {
        _db.HoroscopeQueryHistory.Add(history);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Devuelve el signo con más consultas registradas.
    /// </summary>
    public async Task<(string Sign, int Count)?> GetMostQueriedSignAsync()
    {
        var result = await _db.HoroscopeQueryHistory
            .GroupBy(h => h.Sign)
            .Select(g => new { Sign = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        return result is null ? null : (result.Sign, result.Count);
    }
}
