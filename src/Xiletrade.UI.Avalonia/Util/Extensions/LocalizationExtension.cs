using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics.CodeAnalysis;
using Xiletrade.Library.ViewModels;

namespace Xiletrade.UI.Avalonia.Util.Extensions;

// not AOT compatible
public class LocalizationExtension : MarkupExtension
{
    public string Key { get; set; }

    public LocalizationExtension(string key)
    {
        Key = key;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding($"[{Key}]") { Source = TranslationViewModel.Instance };
    }
}