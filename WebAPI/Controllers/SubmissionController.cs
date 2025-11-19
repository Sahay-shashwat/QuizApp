using Application.DTO;
using Application.Service;
using Core.Common;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using StackExchange.Redis;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionController : ControllerBase
{
  private readonly SubmissionService _submissionService;
  private readonly IDatabase _database;
  private readonly ActivitySource _activitySource;
  private readonly Meter _meter;

  public SubmissionController(SubmissionService submissionService, IConnectionMultiplexer redis, ActivitySource activitySource, Meter meter)
  {
    _submissionService = submissionService;
    _database = redis.GetDatabase();
    _activitySource = activitySource;
    _meter = meter;
  }

  [HttpPost]
  [Authorize(Roles = "Player")]
  [ServiceFilter(typeof(AntiCheatHeaderFilter))]
  [ServiceFilter(typeof(RequestTimeOutFilter))]
  public async Task<IActionResult> Submission([FromBody] SubmissionDTO dto)
  {
    try
    {
      using var activity = _activitySource.StartActivity($"SubmitAnswer:{dto.UserId}", ActivityKind.Server);
      activity?.SetTag("UserId", dto.UserId);
      activity?.SetTag("QuestionId", dto.QuestionID);
      activity?.SetTag("Answer", dto.SelectedOptionId);
      activity?.SetTag("Status", "Submission Requested");
      dynamic data = await _submissionService.AddAsync(dto);

      if (data is OperationResult<Submission> && !data.Success)
      {
        return StatusCode(data.ErrorCode, data.ErrorMessage);
      }
      await _database.StringSetAsync($"quiz:{dto.UserId}:questionStartTime", DateTime.UtcNow.ToString(), TimeSpan.FromMinutes(10));
      return Ok(data);
    }
    catch (Exception ex)
    {
      Log.Error(ex.ToString());
      return BadRequest(ex.ToString());
    }
  }
}
