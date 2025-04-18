using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Timers;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Models;

internal sealed class PricingCooldown // to update its specific to main vm
{
    private static IServiceProvider _serviceProvider;
    
    private static MainViewModel Vm { get; set; }
    private static Timer CooldownTimer { get; } = new(1000);

    private static int TimerValue { get; set; } = 0;
    private static int Search { get; set; } = 0;
    private static int Bulk { get; set; } = 0;
    private static int Fetch { get; set; } = 0;

    internal bool IsEnabled { get { return CooldownTimer.Enabled; } }

    internal PricingCooldown(MainViewModel vm, IServiceProvider serviceProvider)
    {
        Vm = vm;
        _serviceProvider = serviceProvider;
        CooldownTimer.Elapsed += Cooldown_Tick;
    }

    internal static Action cooldownAction = new(() =>
    {
        if (Fetch > 0) --Fetch;
        if (Bulk > 0) --Bulk;
        if (Search > 0) --Search;
        System.Diagnostics.Debug.WriteLine("COOLDOWN - Timer:"+ TimerValue + " Fetch:" + Fetch+" Bulk:" + Bulk + " Search:" + Search);
        if (TimerValue > 0)
        {
            Vm.Form.RateText = Resources.Resources.Main184_rateLimit + " " + TimerValue + "s";
            TimerValue--;
            return;
        }
        Vm.Form.RateText = string.Empty;
        Vm.Form.Freeze = false;
        CooldownTimer.Stop();
    });

    private static void Cooldown_Tick(object sender, EventArgs e)
    {
        _serviceProvider.GetRequiredService<INavigationService>().DelegateActionToUiThread(cooldownAction);
    }

    internal void Update(string timeout)
    {
        string[] timeouts = timeout.Split(':');
        if (timeouts.Length is 4)
        {
            bool ok = int.TryParse(timeouts[1], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out int cd);
            if (ok && cd >= 0) Search = cd;
            ok = int.TryParse(timeouts[2], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out cd);
            if (ok && cd >= 0) Fetch = cd;
            ok = int.TryParse(timeouts[3], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out cd);
            if (ok && cd >= 0) Bulk = cd;
        }
    }

    internal void Apply()
    {
        int[] allCooldowns = [Fetch, Bulk, Search];
        int cooldown = allCooldowns.Max();
        if (cooldown > TimerValue)
        {
            if (CooldownTimer.Enabled)
            {
                CooldownTimer.Stop();
            }

            TimerValue = cooldown;
            CooldownTimer.Start();
            Vm.Form.Freeze = true;

            System.Threading.Thread.Sleep(1000 * (cooldown + 1));
        }
    }
}
