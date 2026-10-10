using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Xiletrade.Library.Services;

namespace Xiletrade.UI.WPF.Util.Extensions;

public class LocalizationExtension(string key) : MarkupExtension
{
    private static readonly LocalizationService _designLocalization;

    public string Key { get; } = key ?? throw new ArgumentNullException(nameof(key));

    static LocalizationExtension() // fix for XAML designer
    {
        var isInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        if (isInDesignMode)
        {
            _designLocalization = new(null, null); 
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider) // do not use this provider
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