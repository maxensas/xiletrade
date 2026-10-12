using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Xiletrade.Library.Services.Extension;

public static class ServiceProviderExtensions
{
    public static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        this IServiceProvider provider, params object[] parameters) => ActivatorUtilities.CreateInstance<T>(provider, parameters);
}
