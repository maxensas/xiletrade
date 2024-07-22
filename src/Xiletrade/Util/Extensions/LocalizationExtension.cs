using System.Windows.Data;
using Xiletrade.Library.ViewModels;

namespace Xiletrade.Util.Extensions;

public class LocalizationExtension : Binding
{
    public LocalizationExtension(string name) : base("[" + name + "]")
    {
        Mode = BindingMode.OneWay;
        Source = TranslationViewModel.Instance;
    }
}
