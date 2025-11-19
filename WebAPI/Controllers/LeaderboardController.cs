using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[Controller]")]
  public class LeaderboardController : Controller
  {
    private readonly ILeaderboardService _leaderboardService;
    public LeaderboardController(ILeaderboardService leaderboardService)
    {
      _leaderboardService = leaderboardService;
    }

    [HttpGet("quiz/{quizId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLeaderboard(int quizId)
    {
      try
      {
        var leaderboard = await _leaderboardService.GetLeaderboardForQuizAsync(quizId);
        return Ok(leaderboard);
      }
      catch (Exception ex)
      {
        Log.Error(ex.Message);
        return BadRequest(ex.Message);
      }
    }
  }
}
