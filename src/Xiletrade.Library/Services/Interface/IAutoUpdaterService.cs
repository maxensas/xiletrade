using System.Threading.Tasks;

namespace Xiletrade.Library.Services.Interface;

public interface IAutoUpdaterService
{
    Task CheckUpdateAsync(bool manualCheck = false);
}