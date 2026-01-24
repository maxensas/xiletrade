using System.Text;
using Xunit.Abstractions;

namespace Xiletrade.Test.Fuzzy;

public class UnitTestLevDistanceEn : UnitTest
{
    private readonly string _sourceString = "Spark fires 3 additional Projectiles";
    private readonly string _expectedString = "Spark fires an additional Projectile";

    private readonly ITestOutputHelper _output;

    public UnitTestLevDistanceEn(ITestOutputHelper output) : base(0)
    {
        _output = output;
    }

    [Fact]
    public void _01_Fastenshtein_Distance() // algo test
    {
        bool test = true;
        List<int> listScore = new();
        StringBuilder sb = new();

        int levenshteinDistance = Fastenshtein.Levenshtein.Distance(_sourceString, _expectedString);
        listScore.Add(levenshteinDistance);
        sb.Append($"Old  : {_sourceString}");
        sb.AppendLine();
        sb.Append($"New  : {_expectedString}");
        sb.AppendLine();
        sb.Append($"Score distance: {levenshteinDistance}");
        sb.AppendLine();
        sb.AppendLine();

        _output.WriteLine(string.Empty);
        _output.WriteLine(sb.ToString());

        Assert.True(test);
    }

    [Fact]
    public void _01_Raffinert_Levenshtein_Distance() // algo test
    {
        bool test = true;
        List<int> listScore = new();
        StringBuilder sb = new();

        int levenshteinDistance = Raffinert.FuzzySharp.Levenshtein.Distance(_sourceString, _expectedString);
        listScore.Add(levenshteinDistance);
        sb.Append($"Old  : {_sourceString}");
        sb.AppendLine();
        sb.Append($"New  : {_expectedString}");
        sb.AppendLine();
        sb.Append($"Score distance: {levenshteinDistance}");
        sb.AppendLine();
        sb.AppendLine();

        _output.WriteLine(string.Empty);
        _output.WriteLine(sb.ToString());

        Assert.True(test);
    }

    [Fact]
    public void _02_Fastenshtein_Distance_ParserRules() // algo test
    {
        bool test = true;
        List<int> listScore = new();
        StringBuilder sb = new();
        foreach (var mod in ParserDat.Mods)
        {
            if (mod.Replace is "equals")
            {
                int levenshteinDistance = Fastenshtein.Levenshtein.Distance(mod.Old, mod.New);
                listScore.Add(levenshteinDistance);
                sb.Append($"Old  : {mod.Old}");
                sb.AppendLine();
                sb.Append($"New  : {mod.New}");
                sb.AppendLine();
                sb.Append($"Score: {levenshteinDistance}");
                sb.AppendLine();
                sb.AppendLine();
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("Levenshtein distance list : ");
        _output.WriteLine(string.Join(", ", listScore));
        _output.WriteLine("Sum: " + listScore.Sum());
        _output.WriteLine(string.Empty);
        _output.WriteLine("----------------------------------------------");
        _output.WriteLine(sb.ToString());

        Assert.True(test);
    }

    [Fact]
    public void _02_Raffinert_Levenshtein_Distance_ParserRules() // algo test
    {
        bool test = true;
        List<int> listScore = new();
        StringBuilder sb = new();
        foreach (var mod in ParserDat.Mods)
        {
            if (mod.Replace is "equals")
            {
                int levenshteinDistance = Raffinert.FuzzySharp.Levenshtein.Distance(mod.Old, mod.New);
                listScore.Add(levenshteinDistance);
                sb.Append($"Old  : {mod.Old}");
                sb.AppendLine();
                sb.Append($"New  : {mod.New}");
                sb.AppendLine();
                sb.Append($"Score: {levenshteinDistance}");
                sb.AppendLine();
                sb.AppendLine();
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("Levenshtein distance list : ");
        _output.WriteLine(string.Join(", ", listScore));
        _output.WriteLine("Sum: " + listScore.Sum());
        _output.WriteLine(string.Empty);
        _output.WriteLine("----------------------------------------------");
        _output.WriteLine(sb.ToString());

        Assert.True(test);
    }

    [Fact]
    public void _03_Fastenshtein_DistanceFrom_FirstResult() // algo test
    {
        int maxDistance = 5;
        string strOut = string.Empty;
        var entrySeek =
            from result in Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(_sourceString, StringComparison.Ordinal));
        if (seek is null)
        {
            Fastenshtein.Levenshtein lev = new(_sourceString);
            foreach (var item in entrySeek)
            {
                int levenshteinDistance = lev.DistanceFrom(item);
                if (levenshteinDistance <= maxDistance)
                {
                    strOut = item;
                    break;
                }
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("out value : " + strOut);
        _output.WriteLine("expected value : " + _expectedString);

        Assert.Equal(_expectedString, strOut);
    }

    [Fact]
    public void _03_Raffinert_Levenshtein_DistanceFrom_FirstResult() // algo test
    {
        int maxDistance = 5;
        string strOut = string.Empty;
        var entrySeek =
            from result in Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(_sourceString, StringComparison.Ordinal));
        if (seek is null)
        {
            Raffinert.FuzzySharp.Levenshtein lev = new(_sourceString);
            foreach (var item in entrySeek)
            {
                int levenshteinDistance = lev.DistanceFrom(item);
                if (levenshteinDistance <= maxDistance)
                {
                    strOut = item;
                    break;
                }
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("out value : " + strOut);
        _output.WriteLine("expected value : " + _expectedString);

        Assert.Equal(_expectedString, strOut);
    }

    [Fact]
    public void _04_Fastenshtein_DistanceFrom_FullLoop() // algo test
    {
        var strOut = string.Empty;

        int maxDistance = 5;
        var entrySeek =
            from result in Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(_sourceString, StringComparison.Ordinal));
        if (seek is null)
        {
            Fastenshtein.Levenshtein lev = new(_sourceString);
            Dictionary<string, int> dict = new();
            foreach (var item in entrySeek)
            {
                int levenshteinDistance = lev.DistanceFrom(item);
                if (levenshteinDistance <= maxDistance)
                {
                    dict.TryAdd(item, levenshteinDistance);
                }
            }
            strOut = dict.MinBy(kvp => kvp.Value).Key;
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("out value : " + strOut);
        _output.WriteLine("expected value : " + _expectedString);

        Assert.Equal(_expectedString, strOut);
    }

    [Fact]
    public void _04_Raffinert_Levenshtein_DistanceFrom_FullLoop() // algo test
    {
        var strOut = string.Empty;

        int maxDistance = 5;
        var entrySeek =
            from result in Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(_sourceString, StringComparison.Ordinal));
        if (seek is null)
        {
            Raffinert.FuzzySharp.Levenshtein lev = new(_sourceString);
            Dictionary<string, int> dict = new();
            foreach (var item in entrySeek)
            {
                int levenshteinDistance = lev.DistanceFrom(item);
                if (levenshteinDistance <= maxDistance)
                {
                    dict.TryAdd(item, levenshteinDistance);
                }
            }
            strOut = dict.MinBy(kvp => kvp.Value).Key;
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("out value : " + strOut);
        _output.WriteLine("expected value : " + _expectedString);

        Assert.Equal(_expectedString, strOut);
    }
}
