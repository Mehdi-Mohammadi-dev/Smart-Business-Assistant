namespace Vexora.Application.Platform.Statements;

public interface IStatementPdfGenerator
{
    byte[] Generate(CustomerStatementDto statement);
}
