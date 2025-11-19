using Application.DTO;
using Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using StackExchange.Redis;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class QuizController : ControllerBase
  {
    private readonly QuizService _quizService;
    private readonly IDatabase _database;

    public QuizController(QuizService quizService, IConnectionMultiplexer redis)
    {
      _quizService = quizService ?? throw new ArgumentNullException(nameof(quizService));
      _database = redis.GetDatabase();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateQuiz([FromBody] QuizDto dto)
    {
      try
      {
        if (dto == null)
          return BadRequest("Quiz data is required.");

        dynamic data = await _quizService.AddAsync(dto);

        if (!data.Success)
        {
          return StatusCode(data.ErrorCode, data.ErrorMessage);
        }

        return Ok("Quiz created Successfully");
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
        return BadRequest(ex.ToString());
      }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetQuiz(int userId)
    {
      var quiz = await _quizService.GetAllQuiz(userId);

      if (quiz == null)
        return NotFound();

      return Ok(quiz);
    }

    [Authorize(Roles = "Player")]
    [HttpGet("start")]
    public async Task<IActionResult> StartQuiz(int quizId, int userId)
    {
      try
      {
        var question = await _quizService.StartQuizAsync(userId, quizId);

        await _database.StringSetAsync($"quiz:{userId}:questionStartTime", DateTime.UtcNow.ToString(), TimeSpan.FromMinutes(10));
        return Ok(question);
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
        return BadRequest(ex.Message);
      }
    }

  }
}