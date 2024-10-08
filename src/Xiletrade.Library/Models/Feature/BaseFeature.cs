﻿using System;
using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Library.Models.Feature;

internal abstract class BaseFeature
{
    protected static IServiceProvider ServiceProvider { get; private set; }
    protected static ConfigShortcut Shortcut { get; private set; }
    protected static string StringValue { get; private set; }

    internal BaseFeature(IServiceProvider service, ConfigShortcut shortcut, string stringValue = null)
    {
        ServiceProvider = service;
        Shortcut = shortcut;
        StringValue = stringValue;
    }

    internal abstract void Launch();
}