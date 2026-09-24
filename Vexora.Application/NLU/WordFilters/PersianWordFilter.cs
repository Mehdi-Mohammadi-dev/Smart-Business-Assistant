namespace Vexora.Application.NLU.WordFilters;

public sealed class PersianWordFilter : IWordFilter
{
    private static readonly HashSet<string> IgnoredWords =
    [
        "امروز",
        "دیروز",
        "فردا",
        "الان",
        "صبح",
        "شب"
    ];


    public bool IsIgnored(string word)
    {
        return IgnoredWords.Contains(word);
    }
}
