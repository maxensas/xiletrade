using System;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Ninja.Contract;
using Xiletrade.Library.Services;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal abstract record NinjaInfoBase
{
    internal readonly DataManagerService _dm;
    internal readonly PoeNinjaService _ninja;
    internal readonly MainViewModel _vm;

    internal string League { get; set; }
    internal string Name { get; set; }
    internal string Type { get; set; }
    internal string Url { get; set; }
    internal string UrlDetails { get; set; }
    internal string Link { get; set; }
    internal bool VerifiedLink { get; set; }

    internal NinjaInfoBase(DataManagerService dm, PoeNinjaService ninja, MainViewModel vm)
    {
        _dm = dm;
        _ninja = ninja;
        _vm = vm;
    }

    internal static string Normalize(ReadOnlySpan<char> data)
    {
        Span<char> buffer = stackalloc char[data.Length];

        int index = 0;
        foreach (char c in data)
        {
            if (c is '\'' || c is '(' || c is ')')
                continue;

            buffer[index++] = c is ' ' ? '-' : char.ToLowerInvariant(c);
        }

        return new string(buffer[..index]);
    }

    internal abstract Task<NinjaValue> GetNinjaValueAsync();
}
