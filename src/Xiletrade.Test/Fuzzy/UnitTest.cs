using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Poe.Contract;
using Xunit.Abstractions;

namespace Xiletrade.Test.Fuzzy;

public class UnitTest
{
    private readonly ITestOutputHelper _output;

    private readonly string[] _cultures = ["en-US", "ko-KR", "fr-FR", "es-ES", "de-DE", "pt-BR", "ru-RU", "th-TH", "zh-TW"/*, "zh-CN"*/, "ja-JP"];
    private readonly string _path = Path.GetFullPath(@"..\..\..\Fuzzy\Lang\");

    public IEnumerable<string>[] AllFilterEntries { get; private set; }
    public IEnumerable<(string oldMod, string newMod)>[] AllParserMods { get; private set; }

    public UnitTest(ITestOutputHelper output) 
    {
        _output = output;

        LoadAllEntries();
    }

    private void LoadAllEntries()
    {
        AllFilterEntries = new IEnumerable<string>[_cultures.Length];
        AllParserMods = new IEnumerable<(string oldMod, string newMod)>[_cultures.Length];

        JsonHelper serializer = new();

        for(int i = 0; i < _cultures.Length; i++)
        {
            var fullpath = _path + _cultures[i] + @"\" + "ParsingRules.json";
            var fs = new FileStream(fullpath, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                var json = reader.ReadToEnd();
                var parserData = serializer.Deserialize<ParserData>(json);

                List<(string oldMod, string newMod)> mods = new();
                foreach (var mod in parserData.Mods)
                {
                    if (mod.Replace is "equals")
                    {
                        mods.Add((mod.Old, mod.New));
                    }
                }
                AllParserMods[i] = mods;
            }

            fullpath = _path + _cultures[i] + @"\" + "Filters.json";
            fs = new FileStream(fullpath, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                var json = reader.ReadToEnd();
                var filterData = serializer.Deserialize<FilterData>(json);

                AllFilterEntries[i] = from result in filterData.Result
                                from filter in result.Entries
                                select filter.Text;
            }
        }
    }

    public void DisplayDistance(int distance, ReadOnlySpan<char> source, ReadOnlySpan<char> expected)
    {
        _output.WriteLine(string.Empty);
        _output.WriteLine($"Source   : {source}");
        _output.WriteLine($"Expected : {expected}");
        _output.WriteLine($"Levenshtein distance : {distance}");
    }

    public void DisplayDistanceList(List<(int distance, string source, string expected)> listDistance)
    {
        _output.WriteLine("Levenshtein distance list : ");
        _output.WriteLine(string.Join(", ", listDistance.Select(x => x.distance)));
        _output.WriteLine("Sum: " + listDistance.Sum(x => x.distance));
        _output.WriteLine("----------------------------------------------");

        foreach (var group in listDistance)
        {
            DisplayDistance(group.distance, group.source, group.expected);
        }
    }

    public void DisplayResult(int culture, ReadOnlySpan<char> result, ReadOnlySpan<char> expected)
    {
        _output.WriteLine(string.Empty);
        _output.WriteLine($"Language : {_cultures[culture]}");
        _output.WriteLine($"Result   : {result}");
        _output.WriteLine($"Expected : {expected}");
    }
}
