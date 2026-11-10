using System;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

/// <summary>
/// Executes the custom protocol URLs received by the protocol handler services (Windows and Linux).
/// </summary>
/// <remarks>
/// <br/>xiletrade://oauth?code=...&amp;state=... : OAuth token returned by the Xiletrade authentication page.
/// <br/>xiletrade://session?id=... : session cookie sent by the Xiletrade browser extension.
/// </remarks>
internal sealed class ProtocolUrlHandler
{
    private const string Caption = "Protocol Handler";
    private const string SessionCaption = "Token validation";

    private readonly IMessageAdapterService _message;
    private readonly ITokenService _token;
    private readonly INetService _net;
    private readonly DataManagerService _dm;

    internal ProtocolUrlHandler(IMessageAdapterService message, ITokenService token,
        INetService net, DataManagerService dm)
    {
        _message = message;
        _token = token;
        _net = net;
        _dm = dm;
    }

    internal void Handle(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            _message.Show($"Unknown protocol URL: {url}", Caption, MessageStatus.Error);
            return;
        }
        if (uri.Host is "oauth")
        {
            _token.TryInitToken(uri.Query);
            return;
        }
        if (uri.Host is "session")
        {
            InitSession(GetQueryValue(uri.Query, "id"));
            return;
        }
        _message.Show($"Unknown protocol URL: {url}", Caption, MessageStatus.Error);
    }

    /// <summary>
    /// Same result as the session cookie entered in the editor : saved, then taken by the trade client at once.
    /// </summary>
    private void InitSession(ReadOnlySpan<char> id)
    {
        if (!RegexUtil.MD5().IsMatch(id))
        {
            _message.Show("The browser extension sent an invalid session cookie.", SessionCaption, MessageStatus.Error);
            return;
        }

        var current = _token.CustomToken?.AccessToken;
        if (current is not null)
        {
            if (id.Equals(current, StringComparison.OrdinalIgnoreCase))
            {
                _message.Show("Session cookie already up to date.", SessionCaption, MessageStatus.Information);
                return;
            }
            // any web page can open a protocol URL : the replacement of a session is confirmed by the user
            if (!_message.ShowResult("Replace the saved session cookie by the one sent from your browser ?",
                SessionCaption, MessageStatus.Warning, yesNo: true))
            {
                return;
            }
        }

        if (!_token.TryInitToken(id, useCustom: true))
        {
            _message.Show("The session cookie sent from your browser could not be saved.", SessionCaption, MessageStatus.Error);
            return;
        }
        // the trade client takes the new session at once (cookie), no restart
        _net.InitTradeClient(_dm.Config.Options.TimeoutTradeApi);
        _message.Show("Session cookie received from your browser.", SessionCaption, MessageStatus.Information);
    }

    /// <summary>
    /// Value of a parameter of the query ("?a=1&amp;b=2"), empty if missing.
    /// </summary>
    internal static ReadOnlySpan<char> GetQueryValue(ReadOnlySpan<char> query, ReadOnlySpan<char> name)
    {
        query = query.TrimStart('?');
        while (!query.IsEmpty)
        {
            int amp = query.IndexOf('&');
            var pair = amp < 0 ? query : query[..amp];
            int eq = pair.IndexOf('=');
            if (eq > 0 && pair[..eq].Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return pair[(eq + 1)..];
            }
            query = amp < 0 ? ReadOnlySpan<char>.Empty : query[(amp + 1)..];
        }
        return ReadOnlySpan<char>.Empty;
    }
}
