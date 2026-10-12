using System;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Manages token storage, retrieval, and lifecycle.
/// </summary>
public interface ITokenService
{
    OAuthToken CacheToken { get; }
    OAuthToken CustomToken { get; }

    bool TryInitToken(ReadOnlySpan<char> query, bool useCustom = false);
    bool TryGetToken(out string token, bool useCustom = false);
    void LoadTokens();
    void ClearTokens();
}
