using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence
{
  public class UserRepository : IUserRepository
  {
    private readonly QuizDbContext _context;

    public UserRepository(QuizDbContext context)
    {
      _context = context;
    }

    public async Task<OperationResult<User>> AddAsync(User user)
    {
      var result = new OperationResult<User>();
      try
      {
        await _context.AddAsync(user);
        await _context.SaveChangesAsync();
        result.Success = true;
      }
      catch (ValidationException ex)
      {
        Log.Error(ex, "Validation failed.");
        result.Success = false;
        result.ErrorMessage = ex.Message;
        result.ErrorCode = 401;
      }
      catch (DbUpdateException dbEx)
      {
        Log.Error(dbEx, "Database update failed while creating a user.");
        result.Success = false;
        result.ErrorMessage = dbEx.Message;
        result.ErrorCode = 401;
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
        result.Success = false;
        result.ErrorMessage = ex.Message;
        result.ErrorCode = 500;
      }
      return result;
    }

    public async Task<User?> GetUser(String UserName)
    {
      try
      {
        var user = await _context.Users
            .FirstOrDefaultAsync(q => q.UserName == UserName);

        return user;
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
        return null;
      }

    }
  }
}
