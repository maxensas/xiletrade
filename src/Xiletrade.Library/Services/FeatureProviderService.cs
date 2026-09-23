using System;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Hotkey;
using Xiletrade.Library.Services.Extension;
using Xiletrade.Library.Shared.Interop;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Services;

public sealed class FeatureProviderService(IServiceProvider sp, 
    DataManagerService dm, LocalizationService localization)
{
    private readonly IServiceProvider _sp = sp;
    private readonly LocalizationService _localization = localization;
    private readonly DataManagerService _dm = dm;

    internal BaseFeature GetFeature(ConfigShortcut shortcut)
    {
        nint findPoeHwnd = Native.FindWindow(Strings.PoeClass, Strings.PoeCaption);
        bool poeLaunched = findPoeHwnd.ToInt32() > 0;
        bool poeFocused = Native.GetForegroundWindow().Equals(findPoeHwnd);
        string fonction = shortcut.Fonction.ToLowerInvariant();

        _localization.RefreshCurrentCulture();
        // POE is launched and got the focus or in dev mode
        if (poeFocused || _dm.Config.Options.DevMode)
        {
            if (fonction is Strings.Feature.run or Strings.Feature.wiki or Strings.Feature.ninja or Strings.Feature.coe)
            {
                return _sp.CreateInstance<GetItemInfoFeature>(shortcut);
            }
            if (fonction is Strings.Feature.replylast)
            {
                return _sp.CreateInstance<ReplyLastFeature>(shortcut);
            }
            if (fonction is Strings.Feature.syndicate or Strings.Feature.incursion)
            {
                return _sp.CreateInstance<ImagePopupFeature>(shortcut);
            }
            if (fonction is Strings.Feature.tcp)
            {
                return _sp.CreateInstance<KillTcpFeature>(shortcut);
            }
            if (fonction is Strings.Feature.hideout or Strings.Feature.exitchar or Strings.Feature.tradechan
                or Strings.Feature.globalchan or Strings.Feature.invite or Strings.Feature.kick or Strings.Feature.leave
                or Strings.Feature.afk or Strings.Feature.autoreply or Strings.Feature.dnd
                or Strings.Feature.chat1 or Strings.Feature.chat2 or Strings.Feature.chat3
                or Strings.Feature.invlast or Strings.Feature.tradelast or Strings.Feature.whoislast)
            {
                var chatCommand = int.TryParse(shortcut.Value?.ToLowerInvariant(), out int val)
                    ? _dm.Config.ChatCommands.FirstOrDefault(x => x.Id == val).Command : string.Empty;
                return _sp.CreateInstance<SendClipboardFeature>(shortcut, GetChatText(shortcut, chatCommand));
            }
        }

        // POE is launched and do not have the focus
        if (poeLaunched && !poeFocused && fonction is Strings.Feature.whispertrade)
        {
            return _sp.CreateInstance<WhisperTradeFeature>(shortcut);
        }

        // In ALL cases
        return fonction is Strings.Feature.close ? _sp.CreateInstance<CloseFeature>(shortcut)
            : fonction is Strings.Feature.bulk ? _sp.CreateInstance<OpenBulkFeature>(shortcut)
            : fonction is Strings.Feature.config ? _sp.CreateInstance<OpenConfigFeature>(shortcut)
            : fonction is Strings.Feature.regex ? _sp.CreateInstance<OpenRegexManagerFeature>(shortcut)
            : fonction is Strings.Feature.link1 or Strings.Feature.link2 or Strings.Feature.lab or Strings.Feature.poedb ?
                _sp.CreateInstance<StartProcessFeature>(shortcut, GetUrl(shortcut))
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
            : shortcut.Fonction is Strings.Feature.chat1 or Strings.Feature.chat2 or Strings.Feature.chat3
                && chatCommand.Length > 0 ? "/" + chatCommand
            : null;
    }

    private static string GetUrl(ConfigShortcut shortcut)
    {
        return shortcut.Fonction is Strings.Feature.link1 or Strings.Feature.link2 ? shortcut.Value
            : shortcut.Fonction is Strings.Feature.lab ? Strings.Url.Poelab
            : shortcut.Fonction is Strings.Feature.poedb ? Strings.Url.Poedb
            : null;
    }
}
