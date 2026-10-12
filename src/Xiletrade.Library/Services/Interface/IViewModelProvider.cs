using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Main.Form;

namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// ViewModels factory for Xiletrade library
/// </summary>
public interface IViewModelProvider
{
    FormViewModel CreateForm(MainViewModel vm, bool useCustomOrBulk);

    FormViewModel CreateForm(MainViewModel vm, ItemData item, InfoDescription infoDesc, bool showMinMax);

    // can extend
}
