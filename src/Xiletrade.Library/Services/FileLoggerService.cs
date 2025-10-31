using System;
using System.IO;
using System.Threading;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services;

public class FileLoggerService : IFileLoggerService
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public FileLoggerService()
    {
        _filePath = Path.GetFullPath("Xiletrade.log");
    }

    public void Log(string message)
    {
        _semaphore.Wait();
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            File.AppendAllText(_filePath, $"[{timestamp}] {message}{Environment.NewLine}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Log(Exception exception) => Log(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n"
            , exception.Source, exception.Message, exception.StackTrace));

    public void Reset()
    {
        if (File.Exists(_filePath))
            File.Delete(_filePath);
    }
}
