using FluentAssertions;
using Xiletrade.Library.Shared;

namespace Xiletrade.Test.CI.Shared;

/// <summary>
/// Span helpers of <see cref="Extensions"/> must give the same results as the previous string based code.
/// </summary>
public class SpanExtensionsTests
{
    [Theory]
    [InlineData("explicit.stat_1")]
    [InlineData("pseudo.pseudo_total_life")]
    [InlineData("nodot")]
    [InlineData("a.b.c")]
    [InlineData(".x")]
    [InlineData("x.")]
    [InlineData("")]
    public void _01_TryGetStatId_Should_Match_Split(string id)
    {
        var split = id.Split('.');
        id.TryGetStatId(out var stat).Should().Be(split.Length is 2);
        if (split.Length is 2)
        {
            stat.ToString().Should().Be(split[1]);
        }
    }

    [Theory]
    [InlineData("stat_1", true)]
    [InlineData("stat_2", false)]
    [InlineData("", false)]
    public void _02_Span_Lookups_Should_Match_String_Lookups(string key, bool expected)
    {
        var dic = new Dictionary<string, string> { { "stat_1", "value" } };
        dic.ContainsSpanKey(key.AsSpan()).Should().Be(expected);
        dic.TryGetSpanValue(key.AsSpan(), out var value).Should().Be(expected);
        value.Should().Be(expected ? "value" : null);
    }

    [Theory]
    [InlineData("Superior Vaal Regalia", "Superior/Exceptional")]
    [InlineData("Exceptional  Vaal Regalia ", "Superior/Exceptional")]
    [InlineData("Vaal Regalia", "Superior/Exceptional")]
    [InlineData("Foulborn Voll's Protector", "Foulborn")]
    [InlineData("ab abc", "ab/abc")]
    [InlineData("abc ab", "ab/abc")]
    [InlineData("", "x")]
    [InlineData("x", "")]
    public void _03_RemoveStringFromArrayDesc_Should_Match_The_Previous_Code(string input, string variants)
    {
        var array = variants.Split('/');
        var expected = PreviousRemove(input, array);
        input.RemoveStringFromArrayDesc(array).Should().Be(expected);
        input.AsSpan().RemoveStringFromArrayDesc(array).Should().Be(expected);
    }

    // previous implementation
    private static string PreviousRemove(string input, string[] array)
    {
        int maxLen = 0;
        foreach (var s in array)
            if (s.Length > maxLen)
                maxLen = s.Length;

        for (int len = maxLen; len > 0; len--)
        {
            foreach (var s in array)
            {
                if (s.Length != len)
                    continue;
                int idx = input.AsSpan().IndexOf(s.AsSpan());
                if (idx < 0)
                    continue;
                return (input[..idx] + input[(idx + s.Length)..]).Trim();
            }
        }
        return input;
    }
}
