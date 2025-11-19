using Application.Model;
using Application.Service;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly AuthService _authService;

  public AuthController(AuthService authService)
  {
    _authService = authService;
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginModel login)
  {
    try
    {
      dynamic data = await _authService.Login(login);
      if (data.Success)
      {
        return Ok(new { data.Data });
      }
      return StatusCode(data.ErrorCode, data.ErrorMessage);
    }
    catch (Exception ex)
    {
      Log.Error(ex.Message);
      return Unauthorized(ex.Message);
    }

  }
}
