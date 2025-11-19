namespace Core.Common
{
  public class OperationResult<T>
  {
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int? ErrorCode { get; set; }
    public bool Success { get; set; }
  }
}
