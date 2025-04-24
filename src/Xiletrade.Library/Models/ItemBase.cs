using System.Linq;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models;

internal sealed class ItemBase
{
    internal string NameEn { get; set; }
    internal string TypeEn { get; set; }
    internal string Name { get; set; }
    internal string Type { get; set; }

    //public string Rarity;
    internal string[] Inherits { get; set; }

    /// <summary>
    /// Translate item name and type in the correct language used by the trade gateway
    /// </summary>
    /// <param name="item"></param>
    internal void TranslateCurrentItemGateway()
    {
        if (DataManager.Config.Options.Gateway == DataManager.Config.Options.Language)
        {
            return;
        }

        //name
        if (Name.Length > 0 && NameEn.Length > 0)
        {
            var word = DataManager.WordsGateway.FirstOrDefault(x => x.NameEn == NameEn);
            if (word is not null && word.Name.Length > 0 && word.Name.IndexOf('/') is -1)
            {
                Name = word.Name;
            }
        }

        //type
        if (Type.Length > 0 && TypeEn.Length > 0)
        {
            var bases = DataManager.BasesGateway.FirstOrDefault(x => x.NameEn == TypeEn);
            if (bases is not null && bases.Name.Length > 0)
            {
                Type = bases.Name;
            }
            if (bases is null)
            {
                string curId = string.Empty;
                var curIdList = from result in DataManager.Currencies
                                from Entrie in result.Entries
                                where Entrie.Text == Type
                                select Entrie.Id;
                if (curIdList.Any())
                {
                    curId = curIdList.FirstOrDefault();
                }
                if (curId.Length > 0)
                {
                    var curList = from result in DataManager.CurrenciesGateway
                                  from Entrie in result.Entries
                                  where Entrie.Id == curId
                                  select Entrie.Text;
                    if (curList.Any())
                    {
                        Type = curList.FirstOrDefault();
                    }
                }
            }
        }
    }
}
