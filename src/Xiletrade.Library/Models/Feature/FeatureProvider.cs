using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Models.Feature;

/// <summary>
/// Factory used to create and get feature objects.
/// </summary>
internal sealed class FeatureProvider
{
    private static FeatureProvider instance = null;
    private static readonly Lock instancelock = new();

    private FeatureProvider()
    {

    }

    internal static FeatureProvider Instance
    {
        get
        {
            if (instance is null)
            {
                lock (instancelock)
                {
                    instance ??= new FeatureProvider();
                }
            }
            return instance;
        }
    }

    internal static BaseFeature GetFeature(IServiceProvider service, ConfigShortcut shortcut)
    {
        nint findPoeHwnd = Native.FindWindow(Strings.PoeClass, Strings.PoeCaption);
        bool poeLaunched = findPoeHwnd.ToInt32() > 0;
        bool poeFocused = Native.GetForegroundWindow().Equals(findPoeHwnd);
        string fonction = shortcut.Fonction.ToLowerInvariant();

        var dm = service.GetRequiredService<DataManagerService>();
        // POE is launched and got the focus or in dev mode
        if (poeFocused || dm.Config.Options.DevMode)
        {
            if (fonction is Strings.Feature.run or Strings.Feature.wiki or Strings.Feature.ninja)
            {
                return new GetItemInfoFeature(service, shortcut);
            }
            if (fonction is Strings.Feature.replylast)
            {
                return new ReplyLastFeature(service, shortcut);
            }
            if (fonction is Strings.Feature.syndicate or Strings.Feature.incursion)
            {
                return new ImagePopupFeature(service, shortcut);
            }
            if (fonction is Strings.Feature.tcp)
            {
                return new KillTcpFeature(service, shortcut);
            }
            if (fonction is Strings.Feature.hideout or Strings.Feature.exitchar or Strings.Feature.tradechan
                or Strings.Feature.globalchan or Strings.Feature.invite or Strings.Feature.kick or Strings.Feature.leave
                or Strings.Feature.afk or Strings.Feature.autoreply or Strings.Feature.dnd
                or Strings.Feature.chat1 or Strings.Feature.chat2 or Strings.Feature.chat3
                or Strings.Feature.invlast or Strings.Feature.tradelast or Strings.Feature.whoislast)
            {
                var chatCommand = int.TryParse(shortcut.Value.ToLowerInvariant(), out int val) 
                    ? dm.Config.ChatCommands.FirstOrDefault(x => x.Id == val).Command : string.Empty;
                return new SendClipboardFeature(service, shortcut, GetChatText(shortcut, chatCommand));
            }
        }

        // POE is launched and do not have the focus
        if (poeLaunched && !poeFocused && fonction is Strings.Feature.whispertrade) 
        {
            return new WhisperTradeFeature(service, shortcut);
        }

        // In ALL cases
        return fonction is Strings.Feature.close ? new CloseFeature(service, shortcut)
            : fonction is Strings.Feature.bulk ? new OpenBulkFeature(service, shortcut)
            : fonction is Strings.Feature.config ? new OpenConfigFeature(service, shortcut)
            : fonction is Strings.Feature.regex ? new OpenRegexManagerFeature(service, shortcut)
            : fonction is Strings.Feature.link1 or Strings.Feature.link2 or Strings.Feature.lab or Strings.Feature.poedb ?
                new StartProcessFeature(service, shortcut, GetUrl(shortcut))
            : null;
    }

    private static string GetChatText(ConfigShortcut shortcut, string chatCommand)
    {
        return shortcut.Fonction is Strings.Feature.hideout ? Strings.Chat.hideout
            : shortcut.Fonction is Strings.Feature.exitchar ? Strings.Chat.exit
            : shortcut.Fonction is Strings.Feature.leave ? Strings.Chat.leave
            : shortcut.Fonction is Strings.Feature.tradechan ? Strings.Chat.trade + " " + shortcut.Value
            : shortcut.Fonction is Strings.Feature.globalchan ? Strings.Chat.global + " " + shortcut.Value
            : shortcut.Fonction is Strings.Feature.invite ? Strings.Chat.invite + " " + shortcut.Value
            : shortcut.Fonction is Strings.Feature.kick ? Strings.Chat.kick + " " + shortcut.Value
            : shortcut.Fonction is Strings.Feature.afk ? Strings.Chat.afk + " " + shortcut.Value
            : shortcut.Fonction is Strings.Feature.autoreply ? Strings.Chat.autoreply + " " + shortcut.Value
            : shortcut.Fonction is Strings.Feature.dnd ? Strings.Chat.dnd + " " + shortcut.Value
            : shortcut.Fonction is Strings.Feature.invlast ? Strings.Chat.invite
            : shortcut.Fonction is Strings.Feature.tradelast ? Strings.Chat.tradewith
            : shortcut.Fonction is Strings.Feature.whoislast ? Strings.Chat.whois
            : ((shortcut.Fonction is Strings.Feature.chat1 or Strings.Feature.chat2 or Strings.Feature.chat3)
                && chatCommand.Length > 0) ? "/" + chatCommand
            : null;
    }

    private static string GetUrl(ConfigShortcut shortcut)
    {
        return shortcut.Fonction is Strings.Feature.link1 or Strings.Feature.link2 ? shortcut.Value
            : shortcut.Fonction is Strings.Feature.lab ? Strings.UrlPoelab
            : shortcut.Fonction is Strings.Feature.poedb ? Strings.UrlPoedb
            : null;
    }
}
