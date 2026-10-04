namespace Vexora.Application.NLU.Abstractions.Dates;

public interface IDateExtractor
{
    DateTime? Extract(string text);
}
