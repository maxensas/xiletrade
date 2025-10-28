using System;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Ninja.Contract;
using Xiletrade.Library.Models.Ninja.Contract.Two;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.One;
using Xiletrade.Library.Models.Poe.Contract.Two;
using Xiletrade.Library.Models.Prices.Contract;

namespace Xiletrade.Library.Models.Serialization.SourceGeneration;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(AccountData))]
[JsonSerializable(typeof(Armour))]
[JsonSerializable(typeof(ArmourFilters))]
[JsonSerializable(typeof(BaseData))]
[JsonSerializable(typeof(BaseResult))]
[JsonSerializable(typeof(BaseResultData))]
[JsonSerializable(typeof(BulkData))]
[JsonSerializable(typeof(ChatCommands))]
[JsonSerializable(typeof(ConfigChecked))]
[JsonSerializable(typeof(ConfigData))]
[JsonSerializable(typeof(ConfigMods))]
[JsonSerializable(typeof(ConfigOption))]
[JsonSerializable(typeof(ConfigShortcut))]
[JsonSerializable(typeof(CurrencyEntrie))]
[JsonSerializable(typeof(CurrencyResult))]
[JsonSerializable(typeof(CurrencyResultData))]
[JsonSerializable(typeof(DivTiersData))]
[JsonSerializable(typeof(DivTiersResult))]
[JsonSerializable(typeof(ExchangeInfo))]
[JsonSerializable(typeof(FetchData))]
[JsonSerializable(typeof(FetchDataInfo))]
[JsonSerializable(typeof(FetchDataListing))]
[JsonSerializable(typeof(FilterData))]
[JsonSerializable(typeof(FilterResult))]
[JsonSerializable(typeof(FilterResultEntrie))]
[JsonSerializable(typeof(FilterResultOption))]
[JsonSerializable(typeof(FilterResultOptions))]
[JsonSerializable(typeof(Filters))]
[JsonSerializable(typeof(GemData))]
[JsonSerializable(typeof(GemResult))]
[JsonSerializable(typeof(GemResultData))]
[JsonSerializable(typeof(GemTransfigured))]
[JsonSerializable(typeof(ItemInfo))]
[JsonSerializable(typeof(ItemDataApi))]
[JsonSerializable(typeof(JsonData))]
[JsonSerializable(typeof(LeagueData))]
[JsonSerializable(typeof(LeagueResult))]
[JsonSerializable(typeof(LicenceData))]
[JsonSerializable(typeof(Map))]
[JsonSerializable(typeof(MapFilters))]
[JsonSerializable(typeof(MinMax))]
[JsonSerializable(typeof(Misc))]
[JsonSerializable(typeof(MiscFilters))]
[JsonSerializable(typeof(ModOption))]
[JsonSerializable(typeof(NinjaCurDetails))]
[JsonSerializable(typeof(NinjaCurLines))]
[JsonSerializable(typeof(NinjaCurrencyContract))]
[JsonSerializable(typeof(NinjaItemContract))]
[JsonSerializable(typeof(NinjaItemTwoContract))]
[JsonSerializable(typeof(NinjaItemLines))]
[JsonSerializable(typeof(NinjaValue))]
[JsonSerializable(typeof(OfferInfo))]
[JsonSerializable(typeof(OnlineStatus))]
[JsonSerializable(typeof(OptionTxt))]
[JsonSerializable(typeof(ParserData))]
[JsonSerializable(typeof(PoePrices))]
[JsonSerializable(typeof(PriceData))]
[JsonSerializable(typeof(Query))]
[JsonSerializable(typeof(ResultData))]
[JsonSerializable(typeof(Sanctum))]
[JsonSerializable(typeof(SocketFilters))]
[JsonSerializable(typeof(Socket))]
[JsonSerializable(typeof(Sort))]
[JsonSerializable(typeof(Armour))]
[JsonSerializable(typeof(Stats))]
[JsonSerializable(typeof(StatsFilters))]
[JsonSerializable(typeof(Trade))]
[JsonSerializable(typeof(TradeFilters))]
[JsonSerializable(typeof(TypeF))]
[JsonSerializable(typeof(TypeFilters))]
[JsonSerializable(typeof(Ultimatum))]
[JsonSerializable(typeof(UltimatumFilters))]
[JsonSerializable(typeof(Weapon))]
[JsonSerializable(typeof(WeaponFilters))]
[JsonSerializable(typeof(WordData))]
[JsonSerializable(typeof(WordResult))]
[JsonSerializable(typeof(WordResultData))]
[JsonSerializable(typeof(Exchange))]
[JsonSerializable(typeof(ExchangeData))]
[JsonSerializable(typeof(ExchangeStatus))]
[JsonSerializable(typeof(Equipment))]
[JsonSerializable(typeof(EquipmentFilters))]
[JsonSerializable(typeof(FiltersTwo))]
[JsonSerializable(typeof(JsonDataTwo))]
[JsonSerializable(typeof(MapFiltersTwo))]
[JsonSerializable(typeof(MapTwo))]
[JsonSerializable(typeof(MiscFiltersTwo))]
[JsonSerializable(typeof(MiscTwo))]
[JsonSerializable(typeof(QueryTwo))]
[JsonSerializable(typeof(Requirement))]
[JsonSerializable(typeof(RequirementFilters))]
[JsonSerializable(typeof(TradeFiltersTwo))]
[JsonSerializable(typeof(TradeTwo))]
[JsonSerializable(typeof(TypeFiltersTwo))]
[JsonSerializable(typeof(TypeTwo))]
[JsonSerializable(typeof(GitHubAsset))]
[JsonSerializable(typeof(GitHubRelease))]
[JsonSerializable(typeof(NinjaState))]
[JsonSerializable(typeof(NinjaLeagues))]
[JsonSerializable(typeof(DustData))]
[JsonSerializable(typeof(DustLevel))]
public partial class SourceGenerationContext : JsonSerializerContext
{
    internal JsonTypeInfo<T> GetTypeGenerated<T>() where T : class
    {
        JsonTypeInfo info = GetTypeInfo(typeof(T));
        if (info is JsonTypeInfo<T> typedInfo)
            return typedInfo;

        throw new NotSupportedException($"Type {typeof(T).Name} not supported by this context.");
    }
}
