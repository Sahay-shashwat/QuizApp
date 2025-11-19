namespace Core.Entities;

public class User
{
  public int Id { get; set; }
  public string Name { get; set; }
  public string Email { get; set; }
  public string Password { get; set; }
  public string UserName { get; set; }
  public List<Submission> Submissions { get; set; }
  public List<Quiz> Quizs { get; set; }
  public string Role { get; set; }
}

public class UserQuizSession
{
  public int UserId { get; set; }
  public int QuizId { get; set; }
  public int CurrentQuestionIndex { get; set; }
}