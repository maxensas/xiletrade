using System;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class OAuthToken
{
    internal string AccessToken { get; set; }
    //internal string RefreshToken { get; set; }
    internal DateTimeOffset ExpiresAt { get; set; }

    internal OAuthToken(string token, string expires)
    {
        AccessToken = token;
        ExpiresAt = DateTimeOffset.Parse(expires);
    }

    internal bool IsExpired() => DateTimeOffset.UtcNow >= ExpiresAt;

    internal TimeSpan TimeToExpiration() => ExpiresAt - DateTimeOffset.UtcNow;
}
