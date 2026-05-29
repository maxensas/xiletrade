using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xiletrade.Library.Services;

namespace Xiletrade.UI.Avalonia.Util;

public static class Localization
{
    private static IServiceProvider _serviceProvider;
    private static LocalizationService _designLocalization;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    // ---------- Main Property ----------
    public static readonly AttachedProperty<string> KeyProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, string>(
            "Key", typeof(Localization));

    // ---------- ToolTip ----------
    public static readonly AttachedProperty<string> KeyTipProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, string>(
            "KeyTip", typeof(Localization));

    private static readonly Dictionary<AvaloniaObject, IDisposable> _handlers = new();
    
    // ----- Key -----
    public static void SetKey(AvaloniaObject obj, string key)
    {
        obj.SetValue(KeyProperty, key);
        AttachHandler(obj, key, isTooltip: false);
    }

    public static string GetKey(AvaloniaObject obj) => obj.GetValue(KeyProperty);

    // ----- KeyTip -----
    public static void SetKeyTip(AvaloniaObject obj, string keyTip)
    {
        obj.SetValue(KeyTipProperty, keyTip);
        AttachHandler(obj, keyTip, isTooltip: true);
    }

    public static string GetKeyTip(AvaloniaObject obj) => obj.GetValue(KeyTipProperty);

    // ----- Handler common -----
    private static void AttachHandler(AvaloniaObject obj, string key, bool isTooltip)
    {
        if (Design.IsDesignMode && _designLocalization is null)
        {
            _designLocalization = new(null);
        }
        var locService = _designLocalization ?? _serviceProvider.GetRequiredService<LocalizationService>();

        if (string.IsNullOrEmpty(key) || locService is null)
            return;
        
        // Remove old handler if exists
        if (_handlers.TryGetValue(obj, out var oldHandler))
        {
            oldHandler.Dispose();
            _handlers.Remove(obj);
        }

        void UpdateValue(object _, PropertyChangedEventArgs __)
        {
            var value = locService[key];
            if (isTooltip)
                UpdateTooltip(obj, value);
            else
            {
                var prop = DetectMainProperty(obj);
                if (prop != null)
                    obj.SetValue(prop, value);
            }
        }

        locService.PropertyChanged += UpdateValue;

        // Storage for future detachment
        var disposable = new HandlerDisposable(() =>
            locService.PropertyChanged -= UpdateValue);

        _handlers[obj] = disposable;

        // Initial update
        UpdateValue(null, null!);
    }
    
    private static AvaloniaProperty DetectMainProperty(AvaloniaObject obj)
    {
        return obj switch
        {
            TextBlock => TextBlock.TextProperty,
            TextBox => TextBox.TextProperty,
            Button => Button.ContentProperty,
            NativeMenuItem => NativeMenuItem.HeaderProperty,
            ContentControl => ContentControl.ContentProperty,
            // we can extend
            _ => null
        };
    }

    private static void UpdateTooltip(AvaloniaObject obj, string value)
    {
        if (obj is NativeMenuItem native)
        {
            native.SetValue(NativeMenuItem.ToolTipProperty, value);
            return;
        }

        if (obj.GetValue(ToolTip.TipProperty) is ToolTip tt)
        {
            tt.Content = value;
        }
        else
        {
            obj.SetValue(ToolTip.TipProperty, new ToolTip { Content = value });
        }
    }

    private class HandlerDisposable : IDisposable
    {
        private readonly Action _dispose;
        public HandlerDisposable(Action dispose) => _dispose = dispose;
        public void Dispose() => _dispose();
    }
}
