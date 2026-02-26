namespace HoroscopeChallenge.Api.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateOnly BirthDate { get; set; }
    public string PasswordHash { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<HoroscopeQueryHistory> QueryHistory { get; set; } = new List<HoroscopeQueryHistory>();
}
