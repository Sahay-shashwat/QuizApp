using Application.DTO;
using Application.Service;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class UserController : ControllerBase
  {
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
      _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserDto dto)
    {
      try
      {
        if (dto == null)
          return BadRequest("User data is required.");

        dynamic data = await _userService.AddAsync(dto);
        if (data.Success)
        {
          return Ok("User created successfully");
        }

        return StatusCode(data.ErrorCode, data.ErrorMessage);
      }
      catch (Exception ex)
      {
        Log.Error(ex, "An error occurred while creating a user.");
        return BadRequest(ex.Message);
      }
    }

    [HttpGet("{UserName}")]
    public async Task<IActionResult> GetUser(string UserName)
    {
      var user = await _userService.GetUser(UserName);

      if (user == null)
        return NotFound();

      return Ok(user);
    }
  }
}