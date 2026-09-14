using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;

namespace Xiletrade.Library.ViewModels.Whisper;

public sealed partial class WhisperViewModel : ViewModelBase
{
    private readonly ClipboardService _clipboard;

    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    private string charName;

    [ObservableProperty]
    private string labelAccount;

    [ObservableProperty]
    private double viewScale;

    [ObservableProperty]
    private AsyncObservableCollection<WhisperOfferViewModel> offers = new();

    public WhisperViewModel(DataManagerService dm, ClipboardService clipboard,
        Tuple<FetchDataListing, OfferInfo> data)
    {
        _clipboard = clipboard;

        viewScale = dm.Config.Options.Scale;

        message = data.Item1.Whisper;//?.ToString();

        charName = data.Item1.Account.LastCharacterName;
        labelAccount = Resources.Resources.Whisper001_lblAccount + " " + charName;

        if (data.Item1.Offers?.Length > 0)
        {
            Offers.Clear();
            var offers = data.Item2 is not null ? [data.Item2] : data.Item1.Offers;
            //var offers = data.Item1.Offers;
            foreach (var offer in offers)
            {
                var offerVm = new WhisperOfferViewModel()
                {
                    SellerAmount = offer.Item.Amount,
                    SellerCurrency = offer.Item.Currency,
                    GetMessage = offer.Item.Whisper,
                    SellerStock = offer.Item.Stock,

                    BuyerAmount = offer.Exchange.Amount,
                    BuyerCurrency = offer.Exchange.Currency,
                    PayMessage = offer.Exchange.Whisper
                };

                string sellerUri = GetImageUri(dm, offerVm.SellerCurrency);
                string buyerUri = GetImageUri(dm, offerVm.BuyerCurrency);
                if (sellerUri is not null)
                {
                    offerVm.ImageGet = new Uri(sellerUri);
                }
                else
                {
                    offerVm.ImageGet = null;
                    offerVm.LabelGet = GetShortCur(offerVm.SellerCurrency);
                }

                if (buyerUri is not null)
                {
                    offerVm.ImagePay = new Uri(buyerUri);
                }
                else
                {
                    offerVm.ImagePay = null;
                    offerVm.LabelPay = GetShortCur(offerVm.BuyerCurrency);
                }

                offerVm.GetAmount = offerVm.SellerAmount;
                offerVm.PayAmount = Math.Round(offerVm.GetAmount / offerVm.SellerAmount * offerVm.BuyerAmount, 2, MidpointRounding.ToEven);

                if (data.Item1.Offers.Length > 1)
                {
                    offerVm.SliderVisible = true;
                    offerVm.MinimumValue = 0;
                }
                else
                {
                    offerVm.SliderVisible = !(offerVm.SellerStock <= offerVm.SellerAmount);
                    offerVm.MinimumValue = offerVm.SellerAmount;
                }

                Offers.Add(offerVm);
            }
        }
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
    private void SendWhisper(object commandParameter)
    {
        if (Message.Length > 0 && Offers.Count > 0)
        {
            StringBuilder sbWhisper = new(Message);

            List<OfferItem> getList = new(), payList = new();
            foreach (var offer in Offers)
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
            _clipboard.SendWhisperMessage(whisperFormat);
        }
        CloseWindow(commandParameter);
    }

    private static string GetShortCur(string cur)
    {
        int lengthCurMax = 12;
        if (cur.Length > lengthCurMax)
        {
            cur = string.Concat(cur.AsSpan(0, lengthCurMax), "..."); // cur.Substring(0, lengthCurMax) + "...";
        }
        return cur;
    }

    private static string GetImageUri(DataManagerService dm, string curTag)
    {
        var (Entry, _) = dm.Currencies.FindEntryAndGroupIdByCurId(curTag, noCard: true, noMap: true);
        if (Entry is not null)
        {
            var uriCur = Strings.Cdn.Url + Entry.Img;
            return Uri.IsWellFormedUriString(uriCur, UriKind.Absolute) ? uriCur : null;
        }
        return null;
    }
}
