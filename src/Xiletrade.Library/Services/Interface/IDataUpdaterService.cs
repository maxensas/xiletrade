using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.ViewModels.Config;

namespace Xiletrade.Library.Services.Interface;

/// <summary>Service used to update all JSON local data files used by Xiletrade.</summary>
/// <remarks>Retrieve source data from Xiletrade Github repository and GGG server.</remarks>
public interface IDataUpdaterService
{
    Task UpdateAsync(GeneralViewModel cfgVm = null, bool allLanguages = false, bool updateGenerated = true);
    Task<SettingsData> GetAppSettings();
}
