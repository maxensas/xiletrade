using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels.Command;

public sealed partial class WhisperCommand : ViewModelBase
{
    private static WhisperViewModel Vm { get; set; }

    public WhisperCommand(WhisperViewModel vm)
    {
        Vm = vm;
    }

    [RelayCommand]
    private static void CloseWindow(object commandParameter)
    {
        if (commandParameter is IViewBase view)
        {
            view.Close();
        }
    }

    [RelayCommand]
    private static void SendWhisper(object commandParameter)
    {
        if (Vm.Message.Length > 0 && Vm.Offers.Count > 0)
        {
            StringBuilder sbWhisper = new(Vm.Message);

            List<OfferItem> getList = new(), payList = new();
            foreach (var offer in Vm.Offers)
            {
                if (offer.GetAmount > 0 && offer.PayAmount > 0)
                {
                    var containGet = getList.Where(x => x.Message == offer.GetMessage);
                    if (containGet.Any())
                    {
                        containGet.First().Ammount += offer.GetAmount;
                    }
                    else
                    {
                        getList.Add(new OfferItem(offer.GetMessage, offer.GetAmount));
                    }

                    var containPay = payList.Where(x => x.Message == offer.PayMessage);
                    if (containPay.Any())
                    {
                        containPay.First().Ammount += offer.PayAmount;
                    }
                    else
                    {
                        payList.Add(new OfferItem(offer.PayMessage, offer.PayAmount));
                    }
                }
            }

            StringBuilder getWhisper = new(), payWhisper = new();

            bool firstAdd = true;
            foreach (var item in getList)
            {
                if (!firstAdd)
                {
                    getWhisper.Append(", ");
                }
                getWhisper.Append(String.Format(item.Message, item.Ammount));
                firstAdd = false;
            }

            firstAdd = true;
            foreach (var item in payList)
            {
                if (!firstAdd)
                {
                    payWhisper.Append(", ");
                }
                payWhisper.Append(String.Format(item.Message, item.Ammount));
                firstAdd = false;
            }

            /*
            string varPos1 = "{0}", varPos2 = "{1}";
            if (Vm.Offers[0].GetMessage.Contains(varPos1, StringComparison.Ordinal)) // sellerCurrencyWhisper
            {
                sbWhisper.Replace(varPos1, Vm.Offers[0].GetMessage);
            }
            if (Vm.Offers[0].PayMessage.Contains(varPos1, StringComparison.Ordinal)) // buyerCurrencyWhisper
            {
                sbWhisper.Replace(varPos2, Vm.Offers[0].PayMessage.Replace(varPos1, varPos2));
            }
            */

            string whisperFormat = String.Format(sbWhisper.ToString(), getWhisper.ToString(), payWhisper.ToString());
            ClipboardHelper.SendWhisperMessage(whisperFormat);
        }
        CloseWindow(commandParameter);
    }
}
