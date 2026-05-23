using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Xiletrade.Library.Services;

namespace Xiletrade.UI.WPF.Util.Extensions;

public class LocalizationExtension : MarkupExtension
{
    private static LocalizationService _designLocalization;

    public string Key { get; }

    static LocalizationExtension()
    {
        var isInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        if (isInDesignMode)
        {
            _designLocalization = new(null);
        }
    }

    public LocalizationExtension(string key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var source = _designLocalization ?? App.Services.GetRequiredService<LocalizationService>();

        var binding = new Binding($"[{Key}]")
        {
            Source = source,
            Mode = BindingMode.OneWay
        };

        return binding.ProvideValue(serviceProvider);
    }
}