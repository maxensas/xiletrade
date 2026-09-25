using Microsoft.Extensions.Logging;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Services;

/// <summary>Service used to handle behaviours when querying PoE trade APIs.</summary>
/// <remarks>>WIP</remarks>
public sealed class PoeApiService
{
    private readonly UIService _ui;
    private readonly DataManagerService _dm;
    private readonly INetService _net;
    private readonly IMessageAdapterService _message;
    private readonly MainViewModel _vm;

    // Lazzy initialization of the service.
    public PoeApiService(ILogger<PoeApiService> logger, UIService ui, DataManagerService dm, 
        IMessageAdapterService message, INetService net, MainViewModel vm)
    {
        _ui = ui;
        _dm = dm;
        _net = net;
        _message = message;
        _vm = vm;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }
    
    
}
