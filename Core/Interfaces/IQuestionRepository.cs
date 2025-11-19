using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
  public interface IQuestionRepository
  {
    Task<OperationResult<Question>> AddAsync(Question question);
    Task<Question?> GetByIdAsync(int id);
    Task<List<Question>?> GetQuestionsByQuizId(int quizId);
  }
}
