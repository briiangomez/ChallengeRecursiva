using HoroscopeChallenge.Api.DTOs;

namespace HoroscopeChallenge.Api.Services;

public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
