using HoroscopeChallenge.Api.Data;
using HoroscopeChallenge.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoroscopeChallenge.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(int id)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByUsernameAsync(string username)
        => _db.Users.FirstOrDefaultAsync(u => u.Username == username);

    public Task<User?> GetByEmailAsync(string email)
        => _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User> CreateAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }
}
