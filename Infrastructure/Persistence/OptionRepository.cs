using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
  public class OptionRepository : IOptionRepository
  {
    private readonly QuizDbContext _quizDbContext;

    public OptionRepository(QuizDbContext quizDbContext)
    {
      _quizDbContext = quizDbContext;
    }
    public async Task<Option?> GetByIdAsync(int id)
    {

      return await _quizDbContext.Options
          .FirstOrDefaultAsync(q => q.ID == id);
    }
  }
}
