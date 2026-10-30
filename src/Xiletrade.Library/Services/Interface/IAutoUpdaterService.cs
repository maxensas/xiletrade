using System.Threading.Tasks;

namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Checks for available application updates and notifies the user when an update is available.
/// </summary>
public interface IAutoUpdaterService
{
    Task CheckForUpdateAsync(bool manualCheck = false);

    /// <summary>
    /// Deletes the copy of the updater run during the last update (%LOCALAPPDATA%\Xiletrade\Updater), not needed anymore.
    /// </summary>
    Task DeleteUpdaterFolderAsync();
}