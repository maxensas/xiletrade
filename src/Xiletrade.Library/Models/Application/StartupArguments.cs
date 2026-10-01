namespace Xiletrade.Library.Models.Application;

/// <summary>
/// Represents the arguments provided when the application starts.
/// </summary>
/// <param name="Args"></param>
public sealed record StartupArguments(string Args)
{
    public bool HasArgs => !string.IsNullOrEmpty(Args);
}