using System;
using System.IO;
namespace Infrastructure.Logger;
public class FileLogger
{
  private readonly string logFilePath;
  private readonly object lockObj = new object();

  public FileLogger(string filePath)
  {
    logFilePath = filePath;

    // Ensure the directory exists
    var directory = Path.GetDirectoryName(logFilePath);
    if (!Directory.Exists(directory))
    {
      Directory.CreateDirectory(directory);
    }
  }

  public void LogInfo(string message)
  {
    Log("INFO", message);
  }

  public void LogWarning(string message)
  {
    Log("WARNING", message);
  }

  public void LogError(string message)
  {
    Log("ERROR", message);
  }

  private void Log(string level, string message)
  {
    var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
    lock (lockObj)
    {
      File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
    }
  }
}