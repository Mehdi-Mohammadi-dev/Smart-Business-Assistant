using Voxora.Application.NLU.Models;

namespace Voxora.Application.NLU.Abstractions;

public interface ITextNormalizer
{
    NormalizedText Normalize(string text);
}