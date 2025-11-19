using System.ComponentModel.DataAnnotations;

public class OptionDTO
{
  [Required]
  public int ID { get; set; }
  [Required]
  public string Answer { get; set; }
  public bool IsCorrect { get; set; }
}