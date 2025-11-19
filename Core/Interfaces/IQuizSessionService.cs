using Core.Entities;

namespace Core.Interfaces
{
  public interface IQuizSessionService
  {
    Task<UserQuizSession?> GetSessionAsync(int userId);
    Task<string?> GetTokenAsync(int userId);
    Task SaveSessionAsync(UserQuizSession session, string Token);
  }
}
