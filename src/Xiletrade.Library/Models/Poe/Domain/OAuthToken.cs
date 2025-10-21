using System;

namespace Xiletrade.Library.Models.Poe.Domain;

public sealed class OAuthToken
{
    public string AccessToken { get; set; }
    //internal string RefreshToken { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    public OAuthToken()
    {

    }

    public OAuthToken(string token, int expiresDay)
    {
        AccessToken = token;
        ExpiresAt = DateTimeOffset.Now.AddDays(expiresDay);
    }

    public bool IsExpired() => DateTimeOffset.UtcNow >= ExpiresAt;

    public TimeSpan TimeToExpiration() => ExpiresAt - DateTimeOffset.UtcNow;
}
