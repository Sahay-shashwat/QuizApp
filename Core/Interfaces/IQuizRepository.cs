using Core.Entities;
using Core.Common;
namespace Core.Interfaces;

public interface IQuizRepository
{
  //Task<Quiz> GetByIdAsync(int id);
  Task<List<Quiz>> GetAllAsync(int userId);
  Task<OperationResult<Quiz>> AddAsync(Quiz quiz);
  Task<Quiz?> GetByIdAsync(int id);
  //Task UpdateAsync(Quiz quiz);
  //Task DeleteAsync(int id);
}