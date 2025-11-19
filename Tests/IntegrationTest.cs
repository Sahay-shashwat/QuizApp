// File: LeaderboardService.IntegrationTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Features;
using Application.Services;
using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace IntegrationTests
{
  public class InMemorySubmissionRepository : ISubmissionRepository
  {
    private readonly List<Submission> _subs;
    public InMemorySubmissionRepository(List<Submission> subs) => _subs = subs ?? new List<Submission>();

    public Task<OperationResult<Submission>> AddAsync(Submission submission)
    {
      throw new NotImplementedException();
    }

    public Task<List<Submission>> GetAllSubmission()
        => Task.FromResult(_subs.ToList());

    public Task<List<Submission>> GetSubmissionsByQuizIdAsync(int quizId)
        => Task.FromResult(_subs.Where(s => s.Question?.QuizId == quizId || s.Question?.Quiz?.ID == quizId).ToList());

  }
  
  public class FakeQuizSession : UserQuizSession
  {
    public FakeQuizSession(int currentIndex) => CurrentQuestionIndex = currentIndex;
  }

  public class LeaderboardServiceIntegrationTests
  {
    private static Quiz BuildQuiz(int quizId, int questionCount, int marksPerQuestion = 4, DateTime? start = null, TimeSpan? duration = null)
    {
      var s = start ?? DateTime.UtcNow;
      var d = duration ?? TimeSpan.FromMinutes(10);
      var quiz = new Quiz
      {
        ID = quizId,
        StartTime = s,
        EndTime = s.Add(d),
        Questions = Enumerable.Range(1, questionCount)
              .Select(i => new Question
              {
                ID = i,
                QuizId = quizId,
                Marks = marksPerQuestion,
                Options = new List<Option>
                  {
                            new Option { ID = 1, IsCorrect = true },
                            new Option { ID = 2, IsCorrect = false }
                  }
              }).ToList()
      };

      foreach (var q in quiz.Questions) q.Quiz = quiz;
      return quiz;
    }


    private static List<Submission> BuildSubmissionsForUser(int userId, Quiz quiz, bool allCorrect, int spacingSeconds = 20)
    {
      var start = quiz.StartTime;
      var submissions = quiz.Questions
          .Select((q, idx) => new Submission
          {
            UserId = userId,
            Question = q,
            QuestionID = q.ID,
            SelectedOptionId = allCorrect ? 1 : 2,
            SubmittedAt = start.AddSeconds(spacingSeconds * (idx + 1))
          })
          .ToList();
      return submissions;
    }

    private LeaderboardService CreateService(List<Submission> submissions, bool negativeMarking, bool streakBonus, int sessionCurrentQuestion = 1)
    {
      var repo = new InMemorySubmissionRepository(submissions);

      var featureMock = new Mock<IFeatureManager>();
      featureMock.Setup(f => f.IsEnabledAsync("NegativeMarking")).ReturnsAsync(negativeMarking);
      featureMock.Setup(f => f.IsEnabledAsync("StreakBonus")).ReturnsAsync(streakBonus);

      var streak = new StreakBonus();

      var quizSessionMock = new Mock<IQuizSessionService>();
      quizSessionMock.Setup(q => q.GetSessionAsync(It.IsAny<int>()))
          .ReturnsAsync((int uid) => new FakeQuizSession(sessionCurrentQuestion));

      return new LeaderboardService(repo, featureMock.Object, streak, quizSessionMock.Object);
    }

    [Fact(DisplayName = "Integration: Base score and negative marking compute as expected")]
    public async Task Integration_BaseScoreWithNegativeMarking()
    {
      var quiz = BuildQuiz(quizId: 10, questionCount: 5, marksPerQuestion: 4, start: DateTime.UtcNow);
      var subsCorrect = BuildSubmissionsForUser(userId: 1, quiz: quiz, allCorrect: true);
      var subsIncorrect = BuildSubmissionsForUser(userId: 1, quiz: quiz, allCorrect: false);

      var mixed = new List<Submission>();
      mixed.AddRange(subsCorrect.Take(3));
      mixed.AddRange(subsIncorrect.Skip(3).Take(2));
      
      mixed.ForEach(s => s.Question.Quiz = quiz);

      var serviceWithoutNeg = CreateService(mixed, negativeMarking: false, streakBonus: false);
      var serviceWithNeg = CreateService(mixed, negativeMarking: true, streakBonus: false);

      var noNegResult = await serviceWithoutNeg.GetLeaderboardForQuizAsync(quiz.ID);
      var yesNegResult = await serviceWithNeg.GetLeaderboardForQuizAsync(quiz.ID);

      var noNegScore = noNegResult.First().Score;
      var yesNegScore = yesNegResult.First().Score;

      var expectedNoNeg = 3 * 4;
      var expectedYesNeg = expectedNoNeg - (2 * (4.0 / 4.0));

      Assert.Equal(expectedNoNeg, noNegScore);
      Assert.Equal(expectedYesNeg, yesNegScore);
      Assert.True(yesNegScore <= noNegScore);
    }

    [Fact(DisplayName = "Integration: Streak and time bonus are applied and never reduce score")]
    public async Task Integration_StreakAndTimeBonusApplied()
    {
      var quiz = BuildQuiz(quizId: 20, questionCount: 4, marksPerQuestion: 4, start: DateTime.UtcNow, duration: TimeSpan.FromMinutes(4));
     
      var subs = quiz.Questions
          .Select((q, idx) => new Submission
          {
            UserId = 2,
            Question = q,
            QuestionID = q.ID,
            SelectedOptionId = 1,
            SubmittedAt = quiz.StartTime.AddSeconds(5 + idx * 3)
          }).ToList();
      subs.ForEach(s => s.Question.Quiz = quiz);

      var srvNoStreak = CreateService(subs, negativeMarking: false, streakBonus: false);
      var srvWithStreak = CreateService(subs, negativeMarking: false, streakBonus: true);

      var resNoStreak = (await srvNoStreak.GetLeaderboardForQuizAsync(quiz.ID)).First();
      var resWithStreak = (await srvWithStreak.GetLeaderboardForQuizAsync(quiz.ID)).First();

      Assert.Equal(16, resNoStreak.Score); 

      Assert.True(resWithStreak.Score >= resNoStreak.Score, $"With streak/time bonus expected >= {resNoStreak.Score}, got {resWithStreak.Score}");
    }

    [Fact(DisplayName = "Integration: Leaderboard ranks multiple users correctly by score")]
    public async Task Integration_LeaderboardOrderingAcrossUsers()
    {
      
      var quiz = BuildQuiz(quizId: 30, questionCount: 3, marksPerQuestion: 5, start: DateTime.UtcNow);

      var u1 = BuildSubmissionsForUser(1, quiz, allCorrect: true);

      var u2 = BuildSubmissionsForUser(2, quiz, allCorrect: true);
      
      u2[0].SelectedOptionId = 2;

      var u3 = BuildSubmissionsForUser(3, quiz, allCorrect: false);

      var allSubs = new List<Submission>();
      allSubs.AddRange(u1);
      allSubs.AddRange(u2);
      allSubs.AddRange(u3);
      allSubs.ForEach(s => s.Question.Quiz = quiz);

      var service = CreateService(allSubs, negativeMarking: false, streakBonus: false);
      var board = await service.GetLeaderboardForQuizAsync(quiz.ID);

      Assert.Equal(3, board.Count);
      Assert.Equal(1, board[0].UserId);
      Assert.Equal(2, board[1].UserId);
      Assert.Equal(3, board[2].UserId);
    }

    [Fact(DisplayName = "Integration: QuestionNumber is computed from QuizSessionService")]
    public async Task Integration_QuestionNumber_FromQuizSessionService()
    {
      var quiz = BuildQuiz(quizId: 40, questionCount: 5);
      var subs = BuildSubmissionsForUser(7, quiz, allCorrect: true);
      subs.ForEach(s => s.Question.Quiz = quiz);

      var repo = new InMemorySubmissionRepository(subs);
      var featureMock = new Mock<IFeatureManager>();
      featureMock.Setup(f => f.IsEnabledAsync("NegativeMarking")).ReturnsAsync(false);
      featureMock.Setup(f => f.IsEnabledAsync("StreakBonus")).ReturnsAsync(false);

      var streak = new StreakBonus();

      var quizSessionMock = new Mock<IQuizSessionService>();
      quizSessionMock.Setup(q => q.GetSessionAsync(It.IsAny<int>()))
          .ReturnsAsync(new FakeQuizSession(3));

      var service = new LeaderboardService(repo, featureMock.Object, streak, quizSessionMock.Object);

      var board = await service.GetLeaderboardForQuizAsync(quiz.ID);
      var entry = board.First();

      Assert.Equal(7, entry.UserId);
      Assert.Equal(2, entry.QuestionNumber);
    }
  }
}
