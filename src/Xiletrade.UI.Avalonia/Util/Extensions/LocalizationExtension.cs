using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Services;

namespace Xiletrade.UI.Avalonia.Util.Extensions;

// NOT USED
public class LocalizationExtension : MarkupExtension
{
    private static LocalizationService _designLocalization;

    public string Key { get; }

    public LocalizationExtension(string key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
    }

    //[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public override object ProvideValue(IServiceProvider serviceProvider) // TODO check if we can trim in AOT
    {
        if (Design.IsDesignMode && _designLocalization is null)
        {
            _designLocalization = new(null);
        }
        var source = _designLocalization ?? App.Services.GetRequiredService<LocalizationService>();

        return new Binding
        {
            Path = $"[{Key}]",
            Source = source,
            Mode = BindingMode.OneWay
        };
    }
}