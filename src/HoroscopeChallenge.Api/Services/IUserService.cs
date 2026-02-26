using HoroscopeChallenge.Api.DTOs;

namespace HoroscopeChallenge.Api.Services;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(int userId);
    Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
}
