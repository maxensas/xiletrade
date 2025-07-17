using System.Threading.Tasks;

namespace Xiletrade.Library.Services.Interface;

public interface IAutoUpdaterService
{
    Task CheckUpdate(bool manualCheck = false);
}