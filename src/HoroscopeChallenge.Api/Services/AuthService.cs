using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HoroscopeChallenge.Api.Domain.Entities;
using HoroscopeChallenge.Api.DTOs;
using HoroscopeChallenge.Api.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace HoroscopeChallenge.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _users.GetByUsernameAsync(request.Username) is not null)
            throw new InvalidOperationException("El username ya está en uso.");

        if (await _users.GetByEmailAsync(request.Email) is not null)
            throw new InvalidOperationException("El email ya está registrado.");

        var user = new User
        {
            Username     = request.Username.Trim(),
            Email        = request.Email.Trim().ToLower(),
            BirthDate    = request.BirthDate,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt    = DateTime.UtcNow
        };

        await _users.CreateAsync(user);
        return new LoginResponse(GenerateToken(user), user.Username, user.Email);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _users.GetByUsernameAsync(request.Username)
            ?? throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        return new LoginResponse(GenerateToken(user), user.Username, user.Email);
    }

    private string GenerateToken(User user)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds      = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires    = DateTime.UtcNow.AddHours(double.Parse(jwtSection["ExpiresInHours"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username",                    user.Username)
        };

        var token = new JwtSecurityToken(
            issuer:             jwtSection["Issuer"],
            audience:           jwtSection["Audience"],
            claims:             claims,
            expires:            expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
