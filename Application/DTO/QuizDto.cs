using System.ComponentModel.DataAnnotations;

namespace Application.DTO
{
  public class QuizDto
  {
    [Required]
    public int ID { get; set; }
    [Required]
    public string Name { get; set; }
    public int UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }
    public List<QuestionDTO> Questions { get; set; }
  }
}