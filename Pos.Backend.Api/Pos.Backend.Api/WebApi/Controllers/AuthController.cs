using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Backend.Api.Core.DTOs;
using Pos.Backend.Api.Core.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Pos.Backend.Api.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly JwtService _jwt;

    public AuthController(AuthService auth, JwtService jwt)
    {
        _auth = auth;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var (ok, error) = await _auth.RegisterAsync(dto);
        if (!ok) return BadRequest(new { error });

        return Ok(new { message = "REGISTER_OK" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _auth.ValidateLoginAsync(dto);

        if (user == null)
            return Unauthorized();

        var token = _jwt.GenerateToken(user);

        return Ok(new { token });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var username = User.FindFirst("username")?.Value;
        var companyId = User.FindFirst("companyId")?.Value;
        var establishmentId = User.FindFirst("establishmentId")?.Value;
        var emissionPointId = User.FindFirst("emissionPointId")?.Value;

        return Ok(new
        {
            userId,
            username,
            companyId,
            establishmentId,
            emissionPointId
        });
    }
}
