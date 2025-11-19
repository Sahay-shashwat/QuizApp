public class ExceptionHandlingMiddleware
{
  private readonly RequestDelegate _next;

  public ExceptionHandlingMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task Invoke(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      context.Response.StatusCode = 500;
      context.Response.ContentType = "application/json";

      var response = new
      {
        Message = "An unexpected error occurred. Please contact support.",
        Details = ex.Message // Optional: hide in production
      };

      await context.Response.WriteAsJsonAsync(response);
    }
  }
}
