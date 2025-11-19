using Application.Services;
using Core.Entities;
using Core.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.FeatureManagement;
using Xunit;
using Moq;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

public class LeaderboardPropertyTests
{
  // --------------------------
  // Helper: Generate a quiz with submissions
  // --------------------------
  private static (List<Submission> submissions, Quiz quiz) GenerateRandomQuizData(int userId, int questionCount, bool allCorrect)
  {
    var start = DateTime.UtcNow;
    var quiz = new Quiz
    {
      ID = 1,
      StartTime = start,
      EndTime = start.AddMinutes(10),
      Questions = Enumerable.Range(1, questionCount)
            .Select(i => new Question
            {
              ID = i,
              Marks = 4,
              QuizId = 1,
              Options = new List<Option>
                {
                        new Option { ID = 1, IsCorrect = true },
                        new Option { ID = 2, IsCorrect = false }
                }
            }).ToList()
    };

    quiz.Questions.ForEach(q => q.Quiz = quiz);

    var submissions = quiz.Questions
        .Select((q, i) => new Submission
        {
          UserId = userId,
          Question = q,
          QuestionID = q.ID,
          SelectedOptionId = allCorrect ? 1 : 2,
          SubmittedAt = start.AddSeconds(20 * (i + 1))
        })
        .ToList();

    return (submissions, quiz);
  }

  private LeaderboardService CreateService(
      List<Submission> submissions,
      bool negativeMarking,
      bool streakBonus)
  {
    var repo = new Mock<ISubmissionRepository>();
    repo.Setup(r => r.GetSubmissionsByQuizIdAsync(It.IsAny<int>()))
        .ReturnsAsync(submissions);

    var feature = new Mock<IFeatureManager>();
    feature.Setup(f => f.IsEnabledAsync("NegativeMarking")).ReturnsAsync(negativeMarking);
    feature.Setup(f => f.IsEnabledAsync("StreakBonus")).ReturnsAsync(streakBonus);

    var streak = new Application.Features.StreakBonus();

    var quizSession = new Mock<IQuizSessionService>();

    return new LeaderboardService(submissionRepository : repo.Object, featureFlagService : feature.Object, streakBonus : streak, quizSessionService : quizSession.Object);
  }

  // --------------------------------------------------------------------
  // 1. Without negative marking, score must never be negative
  // --------------------------------------------------------------------
  [Property(MaxTest = 200)]
  public async Task ScoreIsNeverNegativeWithoutNegativeMarking(int questionCount)
  {
    questionCount = Math.Clamp(questionCount, 1, 20);

    var (subs, quiz) = GenerateRandomQuizData(1, questionCount, allCorrect: false);

    var service = CreateService(subs, negativeMarking: false, streakBonus: false);

    var result = await service.GetLeaderboardForQuizAsync(1);
    var score = result.First().Score;

    Assert.True(score >= 0, $"Score was negative: {score}");
  }
  
  // --------------------------------------------------------------------------------
  // 5. Without negative marking and all incorrect answer, score must be zero
  // --------------------------------------------------------------------------------
  [Property(MaxTest = 200)]
  public async Task ScoreIsZeroWithoutNegativeMarkingAndIncorrectAnswer(int questionCount)
  {
    questionCount = Math.Clamp(questionCount, 1, 20);

    var (subs, quiz) = GenerateRandomQuizData(1, questionCount, allCorrect: false);

    var service = CreateService(subs, negativeMarking: false, streakBonus: false);

    var result = await service.GetLeaderboardForQuizAsync(1);
    var score = result.First().Score;

    Assert.True(score == 0, $"Score was not zero: {score}");
  }

  // -------------------------------------------------------------------------------------------
  // 2. Enabling negative marking must never a score greater than Negative Marking disabled
  // -------------------------------------------------------------------------------------------
  [Property(MaxTest = 200)]
  public async Task NegativeMarkingShouldNeverIncreaseScore(int questionCount)
  {
    questionCount = Math.Clamp(questionCount, 1, 20);

    var (subs, quiz) = GenerateRandomQuizData(1, questionCount, allCorrect: false);

    var noNeg = await CreateService(subs, false, false).GetLeaderboardForQuizAsync(1);
    var yesNeg = await CreateService(subs, true, false).GetLeaderboardForQuizAsync(1);

    Assert.True(yesNeg.First().Score <= noNeg.First().Score,
        "Negative marking increased score");
  }

  // --------------------------------------------------------------------
  // 3. Streak bonus must never DECREASE score
  // --------------------------------------------------------------------
  [Property(MaxTest = 200)]
  public async Task StreakBonusNeverDecreasesScore(int questionCount)
  {
    questionCount = Math.Clamp(questionCount, 1, 20);

    var (subs, quiz) = GenerateRandomQuizData(1, questionCount, allCorrect: true);

    var noStreak = await CreateService(subs, false, false).GetLeaderboardForQuizAsync(1);
    var yesStreak = await CreateService(subs, false, true).GetLeaderboardForQuizAsync(1);

    Assert.True(yesStreak.First().Score >= noStreak.First().Score,
        "Streak bonus decreased score");
  }

  // --------------------------------------------------------------------
  // 4. Shuffling submissions must NOT change final score
  // --------------------------------------------------------------------
  [Property(MaxTest = 200)]
  public async Task ShufflingSubmissionsShouldNotChangeScore(int questionCount)
  {
    questionCount = Math.Clamp(questionCount, 1, 20);

    var (subs, quiz) = GenerateRandomQuizData(1, questionCount, allCorrect: true);

    var shuffled = subs.OrderBy(_ => Guid.NewGuid()).ToList();

    var s1 = await CreateService(subs, false, true).GetLeaderboardForQuizAsync(1);
    var s2 = await CreateService(shuffled, false, true).GetLeaderboardForQuizAsync(1);

    Assert.Equal(s1.First().Score, s2.First().Score);
  }
}
