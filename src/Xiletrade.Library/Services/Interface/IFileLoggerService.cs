using System;

namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Provides file-based logging operations.
/// </summary>
public interface IFileLoggerService
{
    void Log(string message);
    void Log(Exception exception);
    void Reset();
}
