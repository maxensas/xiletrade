using System;

namespace Xiletrade.Library.Services.Interface;

public interface IClipboardService
{
    void Clear();
    void SetClipboard(string data);
    string GetClipboard(bool clear = false);
    bool ContainsUnicodeTextData();
    bool ContainsTextData();
    bool ContainsAnyTextData();
    void SendClipboardCommand(string command);
    void SendClipboardCommandLastWhisper(string command);
    void SendWhisperMessage(ReadOnlySpan<char> message);
    void SendRegex(string message);
}
