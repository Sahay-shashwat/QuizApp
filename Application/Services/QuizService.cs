using Application.DTO;
using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Application.Service;

public class QuizService
{
  private readonly IQuizRepository _quizRepository;
  private readonly IQuizSessionService _quizSessionService;

  public QuizService(IQuizRepository quizRepository, IQuizSessionService quizSessionService)
  {
    _quizRepository = quizRepository;
    _quizSessionService = quizSessionService;
  }

  public async Task<OperationResult<Quiz>> AddAsync(QuizDto dto)
  {
    try
    {
      var quiz = new Quiz
      {
        Name = dto.Name,
        IsActive = dto.IsActive,
        StartTime = dto.StartTime,
        EndTime = dto.EndTime,
        UserId = dto.UserId,
        Questions = new List<Question>()
      };

      if (dto.Questions != null && dto.Questions.Any())
      {
        foreach (var questionDto in dto.Questions)
        {
          var question = new Question
          {
            Title = questionDto.Title,
            Description = questionDto.Description,
            Marks = questionDto.Marks,
            Time = questionDto.Time,
            Options = new List<Option>()
          };

          if (questionDto.Options != null && questionDto.Options.Any())
          {
            foreach (var optionDto in questionDto.Options)
            {
              var option = new Option
              {
                Answer = optionDto.Answer,
                IsCorrect = optionDto.IsCorrect
              };
              question.Options.Add(option);
            }
          }

          quiz.Questions.Add(question);
        }
      }

      var response = await _quizRepository.AddAsync(quiz);
      if (response.Success)
      {
        response.Data = quiz;
      }
      return response;
    }
    catch (Exception ex)
    {
      Log.Error(ex.ToString());
      return new OperationResult<Quiz> { Success = false, ErrorCode = 500, ErrorMessage = ex.Message };
    }
  }

  public async Task<List<QuizDto>?> GetAllQuiz(int userId)
  {
    var data = await _quizRepository.GetAllAsync(userId);
    if (data == null)
      return null;

    List<QuizDto> quizDtos = new List<QuizDto>();

    foreach (Quiz quiz in data)
    {
      var quizDto = new QuizDto
      {
        ID = quiz.ID,
        Name = quiz.Name,
        StartTime = quiz.StartTime,
        EndTime = quiz.EndTime,
        IsActive = quiz.IsActive,
        Questions = quiz.Questions.Select(q => new QuestionDTO
        {
          ID = q.ID,
          Title = q.Title,
          Description = q.Description,
          Marks = q.Marks,
          Time = q.Time,
          Options = q.Options.Select(o => new OptionDTO
          {
            ID = o.ID,
            Answer = o.Answer,
            IsCorrect = o.IsCorrect
          }).ToList()
        }).ToList()
      };
      quizDtos.Add(quizDto);
    }
    return quizDtos;
  }

  public async Task<JsonResult> StartQuizAsync(int userId, int quizId)
  {
    try
    {
      var quiz = await _quizRepository.GetByIdAsync(quizId);
      var Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
      var session = new UserQuizSession { UserId = userId, QuizId = quizId, CurrentQuestionIndex = 0 };
      await _quizSessionService.SaveSessionAsync(session, Token); 
      return new JsonResult(new { Question = quiz!.Questions.First(), anti_cheat = Token });
    }
    catch (Exception ex)
    {
      Log.Error(ex.Message, ex);
      return new JsonResult(new { Question = new Question(), anti_cheat = "" });
    }
  }
}