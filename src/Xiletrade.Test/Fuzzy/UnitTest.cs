using Xiletrade.Benchmark;
using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Test.Fuzzy;

public class UnitTest
{
    public ParserData ParserDat { get; set; }
    private static readonly string[] _culture = ["en-US", "ko-KR", "fr-FR", "es-ES", "de-DE", "pt-BR", "ru-RU", "th-TH", "zh-TW", "zh-CN", "ja-JP"];
    private static readonly string _path = Path.GetFullPath(@"..\..\..\Fuzzy\Rules\");
    public static IJsonSerializer NETSerializer { get; private set; } = new NETJsonSerializer();

    public UnitTest(int culture) 
    {
        string fullpath = _path + _culture[culture] + @"\" + "ParsingRules.json";
        FileStream fs = new FileStream(fullpath, FileMode.Open);
        using StreamReader reader = new(fs);
        fs = null;
        string json = reader.ReadToEnd();
        ParserDat = NETSerializer.Deserialize<ParserData>(json);
    }
}
