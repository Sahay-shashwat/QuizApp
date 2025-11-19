using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
  public class Leaderboard
  {
    public int? UserId { get; set; }
    public double Score { get; set; }
    public int? QuizId { get; set; }
    public int QuestionNumber { get; set; }
  }
}
