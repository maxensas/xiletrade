using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Poe.Domain.Parser;

namespace Xiletrade.Library.Models.Poe.Domain.Interface;

internal interface IJsonDataFactory
{
    byte[] CreateAndSerialize(XiletradeItem xItem, ItemData item, bool useSaleType, string market);
    byte[] CreateAndSerialize(XiletradeItem xItem, UniqueUnidentified unid, string market, string search);
}