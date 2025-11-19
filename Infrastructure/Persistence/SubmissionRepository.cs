using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Authentication;

namespace Infrastructure.Persistence
{
  public class SubmissionRepository : ISubmissionRepository
  {
    private readonly QuizDbContext _quizDbContext;
    private readonly ActivitySource _activitySource;
    public SubmissionRepository(QuizDbContext quizDbContext, ActivitySource activitySource)
    {
      _quizDbContext = quizDbContext;
      _activitySource = activitySource;
    }
    public async Task<OperationResult<Submission>> AddAsync(Submission submission)
    {

      OperationResult<Submission> response = new();
      using var activity = _activitySource.StartActivity($"SubmitActivity:{submission.UserId}", ActivityKind.Internal);
      try
      {
        await _quizDbContext.AddAsync(submission);
        activity?.SetTag("Status", "Submission Saved to db");
        response.Success = true;
        await _quizDbContext.SaveChangesAsync();
      }
      catch (ValidationException ex)
      {
        activity?.SetTag("Status", "Error saving in db");
        Log.Error(ex, "Validation failed.");
        response.Success = false;
        response.ErrorMessage = ex.Message;
        response.ErrorCode = 409;
      }
      catch (DbUpdateException dbEx)
      {
        Log.Error(dbEx, "Database update failed while creating a user.");

        activity?.SetTag("Status", "Error saving in db");
        var errorMessage = dbEx?.InnerException?.Message ?? "Error";
        response.Success = false;
        response.ErrorMessage = errorMessage;
        response.ErrorCode = 409;
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
      
        activity?.SetTag("Status", "Error saving in db");
        response.Success = false;
        response.ErrorMessage = "Error";
        response.ErrorCode = 500;
      }
      return response;
    }

    public async Task<List<Submission>?> GetAllSubmission()
    {
      try
      {
        return await _quizDbContext.Submissions
          .Include(s => s.Question)
            .ThenInclude(q => q.Options)
           .Include(s => s.Question)
            .ThenInclude(q => q.Quiz)
           .ToListAsync();
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
        return null;
      }
    }

    public async Task<List<Submission>?> GetSubmissionsByQuizIdAsync(int quizId)
    {
      try
      {
        return await _quizDbContext.Submissions
          .Include(s => s.Question)
            .ThenInclude(q => q.Options)
          .Include(s => s.Question)
            .ThenInclude(q => q.Quiz)
          .Where(s => s.Question.QuizId == quizId)
          .ToListAsync();
      }
      catch (Exception ex)
      {
        Log.Error(ex, ex.Message);
        return null;
      }
    }
  }
}
