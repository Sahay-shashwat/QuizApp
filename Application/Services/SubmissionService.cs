using Application.DTO;
using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Serilog;
using System.Diagnostics;

namespace Application.Service
{
  public class SubmissionService
  {
    private readonly ISubmissionRepository _repository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ILeaderboardService _leaderBoardService;
    private readonly IRealTimeLeaderboardNotifier _notifier;
    private readonly IQuizSessionService _quizSessionService;
    private readonly IQuizRepository _quizRepository;
    private readonly ActivitySource _activitySource;

    public SubmissionService(ISubmissionRepository repository, IQuestionRepository questionRepository, ILeaderboardService leaderboardService, IRealTimeLeaderboardNotifier notifier, IQuizSessionService quizSessionService, IQuizRepository quizRepository, ActivitySource activitySource)
    {
      _repository = repository;
      _questionRepository = questionRepository;
      _leaderBoardService = leaderboardService;
      _notifier = notifier;
      _quizSessionService = quizSessionService;
      _quizRepository = quizRepository;
      _activitySource = activitySource;
    }

    public async Task<object> AddAsync(SubmissionDTO submission)
    {
      try
      {
        var data = new Submission
        {
          UserId = submission.UserId,
          QuestionID = submission.QuestionID,
          SelectedOptionId = submission.SelectedOptionId,
          SubmittedAt = submission.SubmittedAt,
        };

        var QuestionDetails = await _questionRepository.GetByIdAsync(submission.QuestionID);
        if (QuestionDetails == null) {
          throw new NullReferenceException();
        }

        var quizEndTime = QuestionDetails?.Quiz.EndTime;
        if (quizEndTime == null)
        {
          throw new NullReferenceException();
        }

        if (QuestionDetails?.Options.Find(q => q.ID == submission.SelectedOptionId) == null) { 
          throw new NullReferenceException();
        }

        var id = QuestionDetails?.QuizId;

        if (quizEndTime.Value < submission.SubmittedAt)
        {
          using var activity = _activitySource.StartActivity($"SubmitActivity:{submission.UserId}", ActivityKind.Internal);
          activity?.SetTag("Status", "Quiz ended");
          Log.ForContext("QuizId", id).Information($"Submission {submission.Id} by user {submission.UserId} for question {submission.QuestionID} is done after end of test.");
          return (object)new OperationResult<Submission> { Success = false, ErrorCode = 409, ErrorMessage = "Time's up" };
        }

        var response = await _repository.AddAsync(data);
        if (response.Success)
        {
          response.Data = data;
          await _notifier.NotifyLeaderboardUpdateAsync(data.Question.QuizId);

          var session = await _quizSessionService.GetSessionAsync(submission.UserId);
          var token = await _quizSessionService.GetTokenAsync(submission.UserId);
          var quiz = await _quizRepository.GetByIdAsync(session!.QuizId);

          session.CurrentQuestionIndex++;
          await _quizSessionService.SaveSessionAsync(session, token);

          if (session.CurrentQuestionIndex < quiz!.Questions.Count)
          {
            return new { Question = quiz.Questions[session.CurrentQuestionIndex] };
          }
          return new { Message = "Quiz Complete" };
        }
        return (object)response;
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
        return (object)new OperationResult<Submission> { Success = false, ErrorCode = 500, ErrorMessage = ex.Message };
      }
    }

  }
}
