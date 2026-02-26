using System.Security.Claims;
using HoroscopeChallenge.Api.DTOs;
using HoroscopeChallenge.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoroscopeChallenge.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService) => _userService = userService;

    /// <summary>Devuelve el perfil del usuario autenticado, incluyendo signo zodiacal y días hasta su cumpleaños.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetCurrentUserId();
        try
        {
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        try
        {
            var updated = await _userService.UpdateProfileAsync(userId, request);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirstValue("sub") 
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier) 
               ?? User.FindFirstValue(ClaimTypes.Name)
               ?? throw new InvalidOperationException("No se encontró el claim de identidad del usuario en el token.");
        return int.Parse(sub);
    }
}
