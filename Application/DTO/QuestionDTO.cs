using System.ComponentModel.DataAnnotations;
//namespace Application.DTO;
public class QuestionDTO
{
  [Required]
  public int ID { get; set; }
  [Required]
  public string Title { get; set; }
  public string Description { get; set; }
  public int QuizId { get; set; }
  public int Marks { get; set; }
  public int Time { get; set; }
  public List<OptionDTO> Options { get; set; }
}