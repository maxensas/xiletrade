using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Test.Fuzzy;

public class UnitTest
{
    public ParserData ParserDat { get; private set; }
    public FilterData Filter { get; private set; }
    private static readonly string[] _culture = ["en-US", "ko-KR", "fr-FR", "es-ES", "de-DE", "pt-BR", "ru-RU", "th-TH", "zh-TW", "zh-CN", "ja-JP"];
    private static readonly string _path = Path.GetFullPath(@"..\..\..\Fuzzy\Lang\");
    public static JsonHelper NETSerializer { get; } = new();

    public UnitTest(int culture) 
    {
        string fullpath = _path + _culture[culture] + @"\" + "ParsingRules.json";
        FileStream fs = new FileStream(fullpath, FileMode.Open);
        using (StreamReader reader = new(fs))
        {
            fs = null;
            string json = reader.ReadToEnd();
            ParserDat = NETSerializer.Deserialize<ParserData>(json);
        }

        fullpath = _path + _culture[culture] + @"\" + "Filters.json";
        fs = new FileStream(fullpath, FileMode.Open);
        using (StreamReader reader = new(fs))
        {
            fs = null;
            string json = reader.ReadToEnd();
            Filter = NETSerializer.Deserialize<FilterData>(json);
        }
    }
}
