namespace Xiletrade.Library.Models.Poe.Domain;

public class SearchItem
{
    public string Text { get; set; }
    public string Before { get; set; }
    public string Match { get; set; }
    public string After { get; set; }

    public string FullText => Before + Match + After;
}
