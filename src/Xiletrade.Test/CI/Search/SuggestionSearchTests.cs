using FluentAssertions;
using Xiletrade.Library.ViewModels.Main.Form.Search;

namespace Xiletrade.Test.CI.Search;

/// <summary>
/// Item suggestions of the custom search : texts starting with the query first, then the ones containing it,
/// then the closest ones (one typing error).
/// </summary>
public class SuggestionSearchTests
{
    private static readonly List<(string, bool, string, int)> _items =
    [
        ("Glorious Plate", false, "Armours", 1),
        ("Kaom's Roots", true, "Armours", 1),
        ("Kaom's Heart", true, "Armours", 1),
        ("Heart of the Kaom", true, "Armours", 1)
    ];

    [Theory]
    [InlineData("kaom", "Kaom's Heart")] // starting with the query, alphabetical order
    [InlineData("roots", "Kaom's Roots")] // containing the query
    [InlineData("kaom's heard", "Kaom's Heart")] // one typing error, whatever the case
    public void _01_Enter_Should_Select_The_Best_Suggestion(string query, string expected)
    {
        var search = new SuggestionSearchViewModel(_items) { SearchQuery = query };
        search.SelectFirst(); // searched at once : no suggestion displayed yet
        search.SearchQuery.Should().Be(expected);
    }

    [Fact]
    public void _02_A_Query_Too_Far_Should_Select_Nothing()
    {
        var search = new SuggestionSearchViewModel(_items) { SearchQuery = "zzzz" };
        search.SelectFirst();
        search.SearchQuery.Should().Be("zzzz");
    }
}
