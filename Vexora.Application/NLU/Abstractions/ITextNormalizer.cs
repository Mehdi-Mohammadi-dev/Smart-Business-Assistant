using Vexora.Application.NLU.Models;

namespace Vexora.Application.NLU.Abstractions;

public interface ITextNormalizer
{
    NormalizedText Normalize(string text);
}