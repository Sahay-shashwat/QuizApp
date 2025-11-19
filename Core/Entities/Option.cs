using System.Text.Json.Serialization;

namespace Core.Entities;

public class Option
{
  public int ID { get; set; }
  public string Answer { get; set; }
  public int QuestionId { get; set; }
  [JsonIgnore]
  public Question Question { get; set; }
  public bool IsCorrect { get; set; }
  [JsonIgnore]
  public List<Submission> Submissions { get; set; }
}