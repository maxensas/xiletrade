using System;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Library.Services.Interface;

public interface ITokenService
{
    public OAuthToken CacheToken { get; }
    public OAuthToken CustomToken { get; }

    public bool TryInitToken(ReadOnlySpan<char> query, bool useCustom = false);
    public bool TryGetToken(out string token, bool useCustom = false);
    public void LoadTokens();
    public void ClearTokens();
}
