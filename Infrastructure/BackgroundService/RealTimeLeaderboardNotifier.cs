using Core.Interfaces;
using Infrastructure.RealTimeUpdate;
using Microsoft.AspNetCore.SignalR;

public class RealTimeLeaderboardNotifier : IRealTimeLeaderboardNotifier
{
  private readonly ILeaderboardService _leaderboardService;
  private readonly IHubContext<LeaderBoardHub> _hubContext;

  public RealTimeLeaderboardNotifier(ILeaderboardService leaderboardService, IHubContext<LeaderBoardHub> hubContext)
  {
    _leaderboardService = leaderboardService;
    _hubContext = hubContext;
  }

  public async Task NotifyLeaderboardUpdateAsync(int quizId)
  {
    var leaderboard = await _leaderboardService.GetLeaderboardForQuizAsync(quizId);
    await _hubContext.Clients.Group($"quiz-{quizId}").SendAsync("LeaderBoard", leaderboard);
  }
}