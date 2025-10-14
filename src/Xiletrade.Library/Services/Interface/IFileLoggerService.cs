using System;

namespace Xiletrade.Library.Services.Interface;

public interface IFileLoggerService
{
    void Log(string message);
    void Log(Exception exception);
    void Reset();
}
