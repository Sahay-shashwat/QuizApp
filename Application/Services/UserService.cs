using Application.DTO;
using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace Application.Service
{
  public class UserService
  {
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    { _repository = repository; }

    public async Task<OperationResult<User>> AddAsync(UserDto dto)
    {
      OperationResult<User> result = new OperationResult<User>();
      try
      {
        var hasher = new PasswordHasher<object>();

        string hashedPassword = hasher.HashPassword(dto.UserName, dto.Password);

        var user = new User
        {
          Name = dto.Name,
          Password = hashedPassword,
          Email = dto.Email,
          UserName = dto.UserName,
          Role = dto.Role
        };

        result = await _repository.AddAsync(user);

        if (result.Success)
        {
          result.Data = user;
        }
        return result;
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
        result.Success = false;
        result.ErrorMessage = "Internal Error";
        result.ErrorCode = 500;
        return result;
      }
    }

    public async Task<User?> GetUser(String UserName)
    {
      return await _repository.GetUser(UserName);
    }
  }
}
