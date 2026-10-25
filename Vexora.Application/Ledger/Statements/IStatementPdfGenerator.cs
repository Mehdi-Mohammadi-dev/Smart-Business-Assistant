namespace Vexora.Application.Ledger.Statements;

public interface IStatementPdfGenerator
{
    byte[] Generate(CustomerStatementDto statement);
}
