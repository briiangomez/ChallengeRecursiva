namespace HoroscopeChallenge.Api.Domain.Entities;

public class HoroscopeQueryHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Sign { get; set; } = default!;
    public DateOnly Date { get; set; }
    public string Lang { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = default!;
}
