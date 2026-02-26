namespace HoroscopeChallenge.Api.Domain.Entities;

public class HoroscopeCache
{
    public int Id { get; set; }
    public string Sign { get; set; } = default!;
    public DateOnly Date { get; set; }
    public string Lang { get; set; } = default!;

    public string ResponseJson { get; set; } = default!;
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
