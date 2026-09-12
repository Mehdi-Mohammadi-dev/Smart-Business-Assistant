using Vexora.Application.NLU.Models.Results;

namespace Vexora.Application.NLU.Abstractions.Pipeline;

public interface INluPipeline
{
    NluResult Analyze(string text);
}
