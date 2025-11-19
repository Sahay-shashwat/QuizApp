using Application.Features;
using Core.Entities;
using Core.Interfaces;
using Microsoft.FeatureManagement;
using StackExchange.Redis;

namespace Application.Services
{
  public class LeaderboardService : ILeaderboardService
  {
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IFeatureManager _featureFlagService;
    private readonly StreakBonus _streakBonus;
    private readonly IQuizSessionService _quizSessionService;

    public LeaderboardService(ISubmissionRepository submissionRepository, IFeatureManager featureFlagService, StreakBonus streakBonus, IQuizSessionService quizSessionService)
    {
      _submissionRepository = submissionRepository;
      _featureFlagService = featureFlagService;
      _streakBonus = streakBonus;
      _quizSessionService = quizSessionService;
    }

    public async Task<List<Leaderboard>> GetGlobalLeaderboard()
    {
      var submissions = await _submissionRepository.GetAllSubmission();

      var leaderboard = new List<Leaderboard>();
      if (submissions == null)
      {
        return leaderboard;
      }

      var groups = submissions.GroupBy(s => s.UserId);

      foreach (var group in groups)
      {
        double score = 0;
        foreach (var submission in group)
        {
          bool IsCorrect = submission.Question.Options
              .FirstOrDefault(o => o.ID == submission.SelectedOptionId)?.IsCorrect ?? false;

          if (IsCorrect)
            score += submission.Question.Marks;
          else
          {
            if (await _featureFlagService.IsEnabledAsync("NegativeMarking"))
              score -= submission.Question.Marks / 4.0;
          }

        }
        if (await _featureFlagService.IsEnabledAsync("StreakBonus"))
        {
          score += _streakBonus.CalculateStreakBonus(group.ToList());
        }

        var session = await _quizSessionService.GetSessionAsync(group.Key);

        int QuestionCompleted = (session?.CurrentQuestionIndex ?? 1) - 1;

        leaderboard.Add(new Leaderboard
        {
          UserId = group.Key,
          Score = score,
          QuestionNumber = QuestionCompleted
        });
      }

      return leaderboard.OrderByDescending(e => e.Score).ToList();
    }

    public async Task<List<Leaderboard>> GetLeaderboardForQuizAsync(int quizId)
    {
      var submissions = await _submissionRepository.GetSubmissionsByQuizIdAsync(quizId);

      var leaderboard = new List<Leaderboard>();
      if (submissions == null)
      {
        return leaderboard;
      }

      var groups = submissions.GroupBy(s => s.UserId);

      foreach (var group in groups)
      {
        double score = 0;
        foreach (var submission in group)
        {
          bool IsCorrect = submission.Question.Options
              .FirstOrDefault(o => o.ID == submission.SelectedOptionId)?.IsCorrect ?? false;

          if (IsCorrect)
            score += submission.Question.Marks;
          else
          {
            if (await _featureFlagService.IsEnabledAsync("NegativeMarking"))
              score -= submission.Question.Marks / 4.0;
          }

        }
        if (await _featureFlagService.IsEnabledAsync("StreakBonus"))
        {
          score += _streakBonus.CalculateStreakBonus(group.ToList());
        }

        var session = await _quizSessionService.GetSessionAsync(group.Key);

        int QuestionCompleted = (session?.CurrentQuestionIndex ?? 1) - 1;

        leaderboard.Add(new Leaderboard
        {
          UserId = group.Key,
          Score = score,
          QuizId = quizId,
          QuestionNumber = QuestionCompleted
        });
      }

      return leaderboard.OrderByDescending(e => e.Score).ToList();
    }
  }
}
