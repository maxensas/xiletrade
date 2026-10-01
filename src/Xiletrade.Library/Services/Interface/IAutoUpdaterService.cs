using System.Threading.Tasks;

namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Checks for available application updates and notifies the user when an update is available.
/// </summary>
public interface IAutoUpdaterService
{
    Task CheckForUpdateAsync(bool manualCheck = false);
}