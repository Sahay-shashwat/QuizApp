namespace Core.Entities;

public class Quiz
{
  public int ID { get; set; }
  public int UserId { get; set; }
  public User User { get; set; }
  public string Name { get; set; }
  public DateTime StartTime { get; set; }
  public DateTime EndTime { get; set; }
  public bool IsActive { get; set; }
  public List<Question> Questions { get; set; }
}