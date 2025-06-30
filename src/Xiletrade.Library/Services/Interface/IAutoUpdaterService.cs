namespace Xiletrade.Library.Services.Interface;

public interface IAutoUpdaterService
{
    void CheckUpdate(bool manualCheck = false);
}