using Vexora.Application.Ledger.Dtos;

namespace Vexora.Application.Ledger.Services;

public interface IStatementPdfGenerator
{
    byte[] Generate(CustomerStatementDto statement);
}
