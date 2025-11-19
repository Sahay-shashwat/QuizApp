using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class QuestionController : ControllerBase
  {
    private readonly QuestionService _questionService;

    public QuestionController(QuestionService questionService)
    {
      _questionService = questionService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateQuestion([FromBody] QuestionDTO dto)
    {
      try
      {
        if (dto == null)
          return BadRequest("Question data is required.");

        dynamic data = await _questionService.AddAsync(dto);

        if (!data.Success)
        {
          return StatusCode(data.ErrorCode, data.ErrorMessage);
        }

        return Ok("Question added successfully");
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
        return BadRequest(ex.ToString());
      }
    }
  }
}