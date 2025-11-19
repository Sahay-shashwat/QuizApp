using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence;

public class QuizRepository : IQuizRepository
{
  private readonly QuizDbContext _context;

  public QuizRepository(QuizDbContext context)
  {
    _context = context;
  }

  public async Task<OperationResult<Quiz>> AddAsync(Quiz quiz)
  {
    OperationResult<Quiz> response = new();
    try
    {
      await _context.AddAsync(quiz);      

      Log.ForContext("QuizId", quiz.ID).Information($"Quiz {quiz.Name} created having {(quiz.Questions?.Count ?? 0)} questions");
      await _context.SaveChangesAsync();

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

  public async Task<List<Quiz>> GetAllAsync(int userId)
  {
    try
    {
      var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Options)
            .Where(q => q.UserId == userId)
            .ToListAsync();
      return quiz;
    }
    catch (Exception ex)
    {
      Log.Error(ex.ToString());
      return [];
    }
  }

  public async Task<Quiz?> GetByIdAsync(int id)
  {
    return await _context.Quizzes
        .Include(q => q.Questions)
        .ThenInclude(q => q.Options)
        .FirstOrDefaultAsync(q => q.ID == id);
  }
}