using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Main.Form;

namespace Xiletrade.Library.Services;

internal class ViewModelProvider(DataManagerService dm, IViewManager view, IMessageAdapterService message) : IViewModelProvider
{
    public FormViewModel CreateForm(MainViewModel vm, bool useCustomOrBulk) =>
        new(dm, vm, view, message, useCustomOrBulk);

    public FormViewModel CreateForm(MainViewModel vm, ItemData item, InfoDescription infoDesc, bool showMinMax) =>
        new(dm, vm, view, message, item, infoDesc, showMinMax);
}
