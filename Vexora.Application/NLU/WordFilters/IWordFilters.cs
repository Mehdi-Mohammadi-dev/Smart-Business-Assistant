namespace Vexora.Application.NLU.WordFilters;

public interface IWordFilter
{
    bool IsIgnored(string word);
}
