using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System;
using Xiletrade.Library.ViewModels;

namespace Xiletrade.UI.Avalonia.Util.Extensions;

public class LocalizationExtension : MarkupExtension
{
    public string Key { get; set; }

    public LocalizationExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding($"[{Key}]") { Source = TranslationViewModel.Instance };
    }
}