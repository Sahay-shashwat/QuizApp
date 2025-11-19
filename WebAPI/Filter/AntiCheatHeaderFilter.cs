using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StackExchange.Redis;
using System.Diagnostics;

public class AntiCheatHeaderFilter : IActionFilter
{
  private readonly IDatabase _database;
  private readonly ActivitySource _activitySource;
  public AntiCheatHeaderFilter(IConnectionMultiplexer redis, ActivitySource activitySource)
  {
    _database = redis.GetDatabase();
    _activitySource = activitySource;
  }

  public void OnActionExecuting(ActionExecutingContext context)
  {
    var headers = context.HttpContext.Request.Headers;
    var user = context.HttpContext.User;

    var userId = user.FindFirst("userId")?.Value;
    var headerToken = headers["X-AntiCheat"].ToString();
    using var act = _activitySource.StartActivity("Activity", ActivityKind.Internal);
    act?.SetTag("a", "a");

    var storedToken = _database.StringGet($"session:{userId}:anticheatToken");
    if (string.IsNullOrEmpty(headerToken) || !headerToken.Equals(storedToken))
    {
      using var activity = _activitySource.StartActivity($"SubmitAnswer:{userId}", ActivityKind.Internal);
      activity?.SetTag("UserId", userId);
      activity?.SetTag("Status", "Found without Token");
      context.Result = new UnauthorizedResult();
    }
  }

  public void OnActionExecuted(ActionExecutedContext context)
  { }
}