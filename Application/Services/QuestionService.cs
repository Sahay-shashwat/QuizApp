using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Serilog;

namespace Application.Services
{
  public class QuestionService
  {
    private readonly IQuestionRepository _questionRepository;
    public QuestionService(IQuestionRepository questionRepository)
    {
      _questionRepository = questionRepository;
    }
    public async Task<OperationResult<Question>> AddAsync(QuestionDTO dto)
    {
      try
      {

        var question = new Question
        {
          Title = dto.Title,
          Description = dto.Description,
          QuizId = dto.QuizId,
          Marks = dto.Marks,
          Time = dto.Time,
          Options = new List<Option>()
        };

        if (dto.Options != null && dto.Options.Any())
        {
          foreach (var optionDto in dto.Options)
          {
            var option = new Option
            {
              Answer = optionDto.Answer,
              IsCorrect = optionDto.IsCorrect,
            };
            question.Options.Add(option);
          }
        }
        var response = await _questionRepository.AddAsync(question);
        if (response.Success)
        {
          response.Data = question;
        }
        return response;
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
        return new OperationResult<Question> { Success = false, ErrorCode = 500, ErrorMessage = ex.Message };
      }
    }
  }
}
