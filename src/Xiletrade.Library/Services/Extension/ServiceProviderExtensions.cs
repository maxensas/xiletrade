using Microsoft.Extensions.DependencyInjection;
using System;

namespace Xiletrade.Library.Services.Extension;

public static class ServiceProviderExtensions
{
    public static T CreateInstance<T>(this IServiceProvider provider, params object[] parameters)
        => ActivatorUtilities.CreateInstance<T>(provider, parameters);
}
