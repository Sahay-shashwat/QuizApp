using Core.Common;
using Core.Entities;

namespace Core.Interfaces
{
  public interface ISubmissionRepository
  {
    Task<OperationResult<Submission>> AddAsync(Submission submission);
    Task<List<Submission>?> GetSubmissionsByQuizIdAsync(int quizId);
    Task<List<Submission>?> GetAllSubmission();
  }
}
