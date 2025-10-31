using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Timers;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Services;

/// <summary>Service used to handle behaviours when querying PoE trade APIs.</summary>
public sealed class PoeApiService
{
    private static IServiceProvider _serviceProvider;

    // static members != DI
    private static MainViewModel _vm;
    private static Timer CooldownTimer { get; } = new(1000);

    private static int TimerValue { get; set; } = 0;
    private static int Search { get; set; } = 0;
    private static int Bulk { get; set; } = 0;
    private static int Fetch { get; set; } = 0;

    internal bool IsCooldownEnabled => CooldownTimer.Enabled;

    public PoeApiService(IServiceProvider service)
    {
        _serviceProvider = service;
        _vm = _serviceProvider.GetRequiredService<MainViewModel>();
        CooldownTimer.Elapsed += Cooldown_Tick;
    }

    internal void UpdateCooldown(int[] timeouts)
    {
        if (timeouts.Length is not 4)
        {
            return;
        }
        var searchCd = timeouts[1];
        if (searchCd >= 0) Search = searchCd;
        var fetchCd = timeouts[2];
        if (fetchCd >= 0) Fetch = fetchCd;
        var bulkCd = timeouts[3];
        if (bulkCd >= 0) Bulk = bulkCd;
    }

    internal void ApplyCooldown()
    {
        int cooldown = GetMaxCooldown();
        if (cooldown > TimerValue)
        {
            if (CooldownTimer.Enabled)
            {
                CooldownTimer.Stop();
            }

            TimerValue = cooldown;
            CooldownTimer.Start();
            _vm.Form.Freeze = true;

            System.Threading.Thread.Sleep(1000 * (cooldown + 1));
        }
    }

    private static readonly Action cooldownAction = new(() =>
    {
        DecrementCooldown();

        if (TimerValue > 0)
        {
            _vm.Form.RateText = Resources.Resources.Main184_rateLimit + " " + TimerValue + "s";
            TimerValue--;
            return;
        }
        _vm.Form.RateText = string.Empty;
        _vm.Form.Freeze = false;
        CooldownTimer.Stop();
    });

    private static void Cooldown_Tick(object sender, EventArgs e)
    {
        _serviceProvider.GetRequiredService<INavigationService>().DelegateActionToUiThread(cooldownAction);
    }

    private static int GetMaxCooldown()
    {
        int[] allCooldowns = [Fetch, Bulk, Search];
        return allCooldowns.Max();
    }

    private static void DecrementCooldown()
    {
        if (Fetch > 0) --Fetch;
        if (Bulk > 0) --Bulk;
        if (Search > 0) --Search;
        System.Diagnostics.Debug.WriteLine("COOLDOWN - Timer:" + TimerValue
            + " Fetch:" + Fetch + " Bulk:" + Bulk + " Search:" + Search);
    }

    private static int StringToInt(string timeout)
    {
        bool ok = int.TryParse(timeout, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out int cd);
        return ok ? cd : -1;
    }
}
