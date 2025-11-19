namespace Core.Entities;

public class Submission
{
  public int Id { get; set; }
  public int UserId { get; set; }
  public int QuestionID { get; set; }
  public int SelectedOptionId { get; set; }
  public DateTime SubmittedAt { get; set; }
  public Question Question { get; set; }
}