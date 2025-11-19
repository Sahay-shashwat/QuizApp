using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Authentication;

public class Authenticate : IAuthenticate
{
  private readonly IConfiguration _configuration;

  public Authenticate(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public string? GenerateJwtToken(string username, string role, string userId)
  {
    try
    {
      var claims = new[]
      {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(ClaimTypes.Role, role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("userId" , userId)
      };

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
      var token = new JwtSecurityToken
        (
          issuer: _configuration["Jwt:Issuer"],
          audience: _configuration["Jwt:Audience"],
          claims: claims,
          expires: DateTime.UtcNow.AddHours(1),
          signingCredentials: creds
        );

      var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
      return tokenString;
    }
    catch (Exception ex)
    {
      Log.Error(ex.ToString());
      return null;
    }

  }
}
