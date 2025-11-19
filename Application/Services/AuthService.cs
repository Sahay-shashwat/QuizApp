using Application.Model;
using Core.Common;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace Application.Service
{
  public class AuthService
  {
    private readonly IUserRepository _repository;
    private readonly IAuthenticate _authService;

    public AuthService(IUserRepository repository, IAuthenticate authService)
    {
      _repository = repository;
      _authService = authService;
    }

    public async Task<OperationResult<string>> Login(LoginModel login)
    {
      var res = new OperationResult<string>();
      try
      {
        var user = await _repository.GetUser(login.UserName);

        if (user == null)
        {
          res.ErrorMessage = "Invalid username or password";
          res.ErrorCode = 401;
          res.Success = false;
          return res;
        }

        var hasher = new PasswordHasher<object>();

        if (hasher.VerifyHashedPassword(login.UserName, user.Password, login.Password) == PasswordVerificationResult.Success)
        {
          var token = _authService.GenerateJwtToken(login.UserName, user.Role, (user.Id).ToString());
          if (string.IsNullOrEmpty(token))
          {
            if (user == null)
            {
              res.ErrorMessage = "An unexpected error occurred.";
              res.ErrorCode = 500;
              res.Success = false;
              return res;
            }
          }
          res.Data = token;
          res.Success = true;
          return res;
        }
        else
        {
          res.ErrorMessage = "Invalid username or password";
          res.ErrorCode = 401;
          res.Success = false;
          return res;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex.Message);
        res.ErrorMessage = "An unexpected error occurred.";
        res.ErrorCode = 500;
        res.Success = false;
        return res;
      }
    }

  }
}
