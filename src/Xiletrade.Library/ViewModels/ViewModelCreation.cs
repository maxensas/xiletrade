using System;
using System.Reflection;

namespace Xiletrade.Library.ViewModels;

public enum ViewModelCreation
{
    Manual,     // created with new, outside DI
    Container,  // resolved by the ServiceProvider (registered)
    Factory,    // created by a registered factory
    Activator   // created via ActivatorUtilities with runtime arguments
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ViewModelCreationAttribute(ViewModelCreation mode) : Attribute
{
    public ViewModelCreation Mode { get; } = mode;
}

public static class ViewModelCreationExtensions
{
    // No attribute = Manual
    public static ViewModelCreation GetCreationMode(this Type type) =>
        type.GetCustomAttribute<ViewModelCreationAttribute>()?.Mode ?? ViewModelCreation.Manual;
}