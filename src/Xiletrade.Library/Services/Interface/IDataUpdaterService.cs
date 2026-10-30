using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;

namespace Xiletrade.Library.Services.Interface;

/// <summary>Service used to update all JSON local data files used by Xiletrade.</summary>
/// <remarks>Retrieve source data from Xiletrade Github repository and GGG server.</remarks>
public interface IDataUpdaterService
{
    /// <summary>Downloads the data files, then reloads them when no error occurred.</summary>
    /// <param name="language">Index of the language to update, the language of the configuration when negative.</param>
    /// <returns>True when the data files were reloaded.</returns>
    Task<bool> UpdateAsync(int language = -1, bool allLanguages = false, bool updateGenerated = true);
    Task<SettingsData> GetAppSettings();
}
