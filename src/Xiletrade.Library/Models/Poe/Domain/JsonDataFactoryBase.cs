using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Poe.Domain.Interface;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models.Poe.Domain;

/// <summary>
/// Abstract class used to generate JSON objects for POE 1 and 2 and allow string serialization.
/// </summary>
/// <typeparam name="T">Concrete JSON contract type (<see cref="JsonData"/> or <see cref="JsonDataTwo"/>).</typeparam>
/// <param name="dm"></param>
internal abstract class JsonDataFactoryBase<T>(DataManagerService dm) : IJsonDataFactory where T : class
{
    protected readonly DataManagerService _dm = dm;

    internal abstract T Create(XiletradeItem xItem, UniqueUnidentified unid, string market, string search);

    internal abstract T Create(XiletradeItem xItem, ItemData item, bool useSaleType, string market);

    public string CreateAndSerialize(XiletradeItem xItem, ItemData item, bool useSaleType, string market)
        => _dm.Json.Serialize<T>(Create(xItem, item, useSaleType, market));

    public string CreateAndSerialize(XiletradeItem xItem, UniqueUnidentified unid, string market, string search)
        => _dm.Json.Serialize<T>(Create(xItem, unid, market, search));
}
