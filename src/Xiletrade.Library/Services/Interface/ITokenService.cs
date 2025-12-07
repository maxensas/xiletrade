using System;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Library.Services.Interface;

public interface ITokenService
{
    public OAuthToken CacheToken { get; }
    public bool IsInitialized { get; }

    public bool TryParseQuery(ReadOnlySpan<char> query);
    public bool TryGetToken(out string token);
    public void Load();
    public void Clear();
}
