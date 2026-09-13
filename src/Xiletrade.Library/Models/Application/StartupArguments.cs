namespace Xiletrade.Library.Models.Application;

public sealed record StartupArguments(string Args)
{
    public bool HasArgs => !string.IsNullOrEmpty(Args);
}