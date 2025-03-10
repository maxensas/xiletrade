﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Command;

namespace Xiletrade.Library.ViewModels;

public sealed partial class WhisperViewModel : ViewModelBase
{
    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    private string charName;

    [ObservableProperty]
    private string labelAccount;

    [ObservableProperty]
    private AsyncObservableCollection<WhisperOfferViewModel> offers = new();

    public WhisperCommand Commands { get; private set; }

    public WhisperViewModel(Tuple<FetchDataListing, OfferInfo> data)
    {
        Commands = new(this);

        Message = data.Item1.Whisper;//?.ToString();

        CharName = data.Item1.Account.LastCharacterName;
        LabelAccount = Resources.Resources.Whisper001_lblAccount + " " + CharName;

        if (data.Item1.Offers?.Length > 0)
        {
            Offers.Clear();
            var offers = data.Item2 is not null ? new OfferInfo[] { data.Item2 } : data.Item1.Offers;
            //var offers = data.Item1.Offers;
            foreach (var offer in offers)
            {
                WhisperOfferViewModel offerVm = new();
                offerVm.SellerAmount = offer.Item.Amount;
                offerVm.SellerCurrency = offer.Item.Currency;
                offerVm.GetMessage = offer.Item.Whisper;
                offerVm.SellerStock = offer.Item.Stock;

                offerVm.BuyerAmount = offer.Exchange.Amount;
                offerVm.BuyerCurrency = offer.Exchange.Currency;
                offerVm.PayMessage = offer.Exchange.Whisper;

                string sellerUri = GetImageUri(offerVm.SellerCurrency);
                string buyerUri = GetImageUri(offerVm.BuyerCurrency);
                if (sellerUri != null)
                {
                    offerVm.ImageGet = new Uri(sellerUri);
                }
                else
                {
                    offerVm.ImageGet = null;
                    offerVm.LabelGet = GetShortCur(offerVm.SellerCurrency);
                }

                if (buyerUri != null)
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

    private static string GetShortCur(string cur)
    {
        int lengthCurMax = 12;
        if (cur.Length > lengthCurMax)
        {
            cur = string.Concat(cur.AsSpan(0, lengthCurMax), "..."); // cur.Substring(0, lengthCurMax) + "...";
        }
        return cur;
    }

    private static string GetImageUri(string curTag)
    {
        foreach (CurrencyResultData resDat in DataManager.Currencies)
        {
            if (resDat.Label is not null && resDat.Id is not Strings.CurrencyTypePoe1.Cards && !resDat.Id.Contains(Strings.Maps, StringComparison.Ordinal))
            {
                foreach (CurrencyEntrie entrie in resDat.Entries)
                {
                    if (entrie.Id == curTag && entrie.Img?.ToString().Length > 0)
                    {
                        string uriCur = "https://web.poecdn.com" + entrie.Img.ToString();
                        return Uri.IsWellFormedUriString(uriCur, UriKind.Absolute) ? uriCur : null;
                    }
                }
            }
        }
        return null;
    }
}
