using HoroscopeChallenge.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoroscopeChallenge.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<HoroscopeCache> HoroscopeCache => Set<HoroscopeCache>();
    public DbSet<HoroscopeQueryHistory> HoroscopeQueryHistory => Set<HoroscopeQueryHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).HasMaxLength(50).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();

            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<HoroscopeCache>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Sign).HasMaxLength(30).IsRequired();
            e.Property(h => h.Lang).HasMaxLength(10).IsRequired();
            e.Property(h => h.ResponseJson).IsRequired();
            e.HasIndex(h => new { h.Sign, h.Date, h.Lang }).IsUnique();
        });

        modelBuilder.Entity<HoroscopeQueryHistory>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Sign).HasMaxLength(30).IsRequired();
            e.Property(h => h.Lang).HasMaxLength(10).IsRequired();

            e.HasOne(h => h.User)
             .WithMany(u => u.QueryHistory)
             .HasForeignKey(h => h.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
