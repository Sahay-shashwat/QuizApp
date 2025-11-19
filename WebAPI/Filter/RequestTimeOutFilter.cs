using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StackExchange.Redis;
using System.Diagnostics;
using System.Diagnostics.Metrics;
public class RequestTimeOutFilter : IAsyncActionFilter
{
  private readonly IDatabase _redisDb;
  private readonly TimeSpan _maxAllowedTime;
  private readonly IQuizSessionService _quizSessionService;
  private readonly IQuizRepository _quizRepository;
  private readonly ActivitySource _activitySource;
  private readonly Meter _meter;

  public RequestTimeOutFilter(IConnectionMultiplexer redis, IQuizSessionService quizSessionService, IQuizRepository quizRepository, ActivitySource activitySource, Meter meter)
  {
    _redisDb = redis.GetDatabase();
    _maxAllowedTime = TimeSpan.FromSeconds(30);
    _quizSessionService = quizSessionService;
    _quizRepository = quizRepository;
    _activitySource = activitySource;
    _meter = meter;
  }

  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    var user = context.HttpContext.User;
    var userId = user.FindFirst("userId")?.Value;

    if (string.IsNullOrEmpty(userId))
    {
      context.Result = new UnauthorizedResult();
      return;
    }
    var counter = _meter.CreateUpDownCounter<double>($"answer_per_second");

    var startTimeStr = await _redisDb.StringGetAsync($"quiz:{userId}:questionStartTime");

    if (!DateTime.TryParse(startTimeStr, out var startTime))
    {
      context.Result = new BadRequestObjectResult("Start time not found or invalid.");
      return;
    }

    var currentTime = DateTime.UtcNow;
    var timeTaken = currentTime - startTime;
    counter.Add(1 / timeTaken.TotalSeconds);

    if (timeTaken > _maxAllowedTime)
    {
      using var activity = _activitySource.StartActivity($"SubmitAnswer:{userId}", ActivityKind.Server);
      activity?.SetTag("UserId", userId);
      activity?.SetTag("Status", "Request Time Out");
      context.Result = new BadRequestObjectResult(new
      {
        error = "Time limit exceeded",
        timeTaken = timeTaken.TotalSeconds
      });
      await _redisDb.StringSetAsync($"quiz:{userId}:questionStartTime", DateTime.UtcNow.ToString(), TimeSpan.FromMinutes(10));

      var session = await _quizSessionService.GetSessionAsync(int.Parse(userId));
      var token = await _quizSessionService.GetTokenAsync(int.Parse(userId));
      var quiz = await _quizRepository.GetByIdAsync(session!.QuizId);

      session.CurrentQuestionIndex++;
      await _quizSessionService.SaveSessionAsync(session, token);

      if (session.CurrentQuestionIndex < quiz!.Questions.Count)
      {
        context.Result = new OkObjectResult(new { Question = quiz.Questions[session.CurrentQuestionIndex] });
        return;
      }
      context.Result = new OkObjectResult(new { Message = "Quiz Complete" });
      return;
    }

    var resultContext = await next();
  }
}