using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel.DataAnnotations;
namespace Infrastructure.Persistence
{
  public class QuestionRepository : IQuestionRepository
  {
    private readonly QuizDbContext _quizDbContext;
    public QuestionRepository(QuizDbContext quizDbContext)
    {
      _quizDbContext = quizDbContext;
    }

    public async Task<OperationResult<Question>> AddAsync(Question question)
    {
      OperationResult<Question> response = new OperationResult<Question>();
      try
      {
        await _quizDbContext.AddAsync(question);

        await _quizDbContext.SaveChangesAsync();

        response.Success = true;
      }
      catch (ValidationException ex)
      {
        Log.Error(ex, "Validation failed.");
        response.Success = false;
        response.ErrorMessage = ex.Message;
        response.ErrorCode = 409;
      }
      catch (DbUpdateException dbEx)
      {
        Log.Error(dbEx, "Database update failed while creating a user.");

        var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
        response.Success = false;
        response.ErrorMessage = errorMessage;
        response.ErrorCode = 500;
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
        response.Success = false;
        response.ErrorMessage = ex.ToString();
        response.ErrorCode = 500;
      }
      return response;
    }

    public async Task<Question?> GetByIdAsync(int id)
    {
      try
      {
        var questions = await _quizDbContext.Questions.Include(q => q.Options).Include(q => q.Quiz)
        .FirstOrDefaultAsync(q => q.ID == id);

        return questions;
      }
      catch (Exception ex)
      {
        Log.Error(ex, ex.Message);
        return null;
      }
    }

    public async Task<List<Question>?> GetQuestionsByQuizId(int quizId)
    {
      try
      {
        var questions = await _quizDbContext.Questions.Include(q => q.Options)
          .Where(q => q.QuizId == quizId).ToListAsync();

        return questions;
      }
      catch (Exception ex)
      {
        Log.Error(ex, ex.Message);
        return null;
      }
    }
  }
}
