namespace Core.Interfaces;
public interface IAuthenticate
{
  string? GenerateJwtToken(string username, string Role, string userId);
}