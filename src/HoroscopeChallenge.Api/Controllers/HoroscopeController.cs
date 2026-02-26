using System.Security.Claims;
using HoroscopeChallenge.Api.DTOs;
using HoroscopeChallenge.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoroscopeChallenge.Api.Controllers;

[ApiController]
[Route("api/horoscope")]
[Authorize]
public class HoroscopeController : ControllerBase
{
    private readonly IHoroscopeService _horoscopeService;
    private readonly IConfiguration _config;

    public HoroscopeController(IHoroscopeService horoscopeService, IConfiguration config)
    {
        _horoscopeService = horoscopeService;
        _config           = config;
    }

    [HttpGet("today")]
    [ProducesResponseType(typeof(HoroscopeTodayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetToday([FromQuery] string? lang)
    {
        var userId      = GetCurrentUserId();
        var resolvedLang = lang ?? _config["HoroscopeApi:DefaultLang"] ?? "es";

        try
        {
            var result = await _horoscopeService.GetTodayAsync(userId, resolvedLang);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Error al consultar la API de horóscopo.", detail = ex.Message });
        }
    }

    [HttpGet("most-queried")]
    [ProducesResponseType(typeof(MostQueriedSignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetMostQueried()
    {
        var result = await _horoscopeService.GetMostQueriedSignAsync();
        return result is null ? NoContent() : Ok(result);
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
