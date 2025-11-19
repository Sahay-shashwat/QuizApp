using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using Core.Entities;

namespace Core.Interfaces
{
  public interface IUserRepository
  {
    Task<OperationResult<User>> AddAsync(User user);
    Task<User?> GetUser(string UserName);
  }
}
