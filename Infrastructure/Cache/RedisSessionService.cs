using Core.Entities;
using Core.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

public class RedisSessionService : IQuizSessionService
{
  private readonly IDatabase _database;
  public RedisSessionService(IConnectionMultiplexer redis)
  {
    _database = redis.GetDatabase();
  }

  public async Task<UserQuizSession?> GetSessionAsync(int userId)
  {
    var val = await _database.StringGetAsync($"session:{userId}");
    return val.IsNullOrEmpty ? null : JsonSerializer.Deserialize<UserQuizSession>(val!);
  }

  public async Task<string?> GetTokenAsync(int userId)
  {
    var val = await _database.StringGetAsync($"session:{userId}:anticheatToken");
    return val.IsNullOrEmpty ? null : val.ToString();
  }

  public async Task SaveSessionAsync(UserQuizSession session, string Token)
  {
    var json  = JsonSerializer.Serialize(session);
    await _database.StringSetAsync($"session:{session.UserId}", json, TimeSpan.FromHours(1));
    await _database.StringSetAsync($"session:{session.UserId}:anticheatToken", Token, TimeSpan.FromHours(1));
  }
}