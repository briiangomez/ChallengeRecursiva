using HoroscopeChallenge.Api.Domain.Helpers;
using HoroscopeChallenge.Api.DTOs;
using HoroscopeChallenge.Api.Repositories;

namespace HoroscopeChallenge.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;

    public UserService(IUserRepository users) => _users = users;

    public async Task<UserProfileResponse> GetProfileAsync(int userId)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new UserProfileResponse(
            user.Id,
            user.Username,
            user.Email,
            user.BirthDate,
            ZodiacHelper.GetSign(user.BirthDate),
            ZodiacHelper.GetDaysToBirthday(user.BirthDate, today),
            user.CreatedAt
        );
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        user.Email     = request.Email.Trim().ToLower();
        user.BirthDate = request.BirthDate;

        await _users.UpdateAsync(user);
        return await GetProfileAsync(userId);
    }
}
