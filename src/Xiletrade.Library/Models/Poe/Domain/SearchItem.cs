namespace Xiletrade.Library.Models.Poe.Domain;

public class SearchItem
{
    public SearchFlag Flag { get; set; }
    
    public int Score { get; set; }

    public string Text { get; set; }

    /// <summary>Id of the stat on the trade site (stat suggestions only).</summary>
    public string Id { get; set; }

    /// <summary>Label of the group of the suggestion : kind of stat (pseudo, explicit...) or item category.</summary>
    public string Kind { get; set; }

    /// <summary>Kind of the stat of the trade api : pseudo, explicit, enchant... (stat suggestions only).</summary>
    public string KindType { get; set; }

    /// <summary>Title of a group in the suggestions (kind of stat or item category), not selectable.</summary>
    public bool IsHeader { get; set; }
    public string Before { get; set; }
    public string Match { get; set; }
    public string After { get; set; }
}
