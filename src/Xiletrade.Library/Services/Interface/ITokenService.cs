using System;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Library.Services.Interface;

public interface ITokenService
{
    public OAuthToken CacheToken { get; }

    public bool TryParseQuery(ReadOnlySpan<char> query);
    public void Load();
    public void Clear();
}
