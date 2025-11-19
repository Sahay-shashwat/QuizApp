using Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.RealTimeUpdate
{
  public class LeaderBoardHub : Hub
  {
    private readonly ILeaderboardService _leaderboardService;
    public LeaderBoardHub(ILeaderboardService leaderboardService)
    {
      _leaderboardService = leaderboardService;
    }
    public async Task GetUpdates(int quizId)
    {
      var leaderboard = await _leaderboardService.GetLeaderboardForQuizAsync(quizId);
      await Clients.Group($"quiz-{quizId}").SendAsync("LeaderBoard", leaderboard);
    }
    public async Task JoinQuizGroup(int quizId)
    {
      await Groups.AddToGroupAsync(Context.ConnectionId, $"quiz-{quizId}");
    }
    public async Task JoinGlobalGroup(int quizId)
    {
      await Groups.AddToGroupAsync(Context.ConnectionId, $"global-quiz");
    }

    public async Task GlobalQuiz()
    {
      var leaderboard = await _leaderboardService.GetGlobalLeaderboard();
      await Clients.Group($"global-quiz").SendAsync("Leaderboard", leaderboard);
    }
  }
}
