using System.Text.Json.Serialization;

namespace Core.Entities;

public class Question
{
  public int ID { get; set; }
  public string Title { get; set; }
  public string Description { get; set; }
  public int QuizId { get; set; }
  [JsonIgnore]
  public Quiz Quiz { get; set; }
  public List<Option> Options { get; set; }
  public int Marks { get; set; }
  public int Time { get; set; }
  [JsonIgnore]
  public List<Submission> Submissions { get; set; }
}