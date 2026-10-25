using System.Text;
using FluentAssertions;
using Vexora.Application.Platform.Statements;
using Vexora.Infrastracture.Pdf;
using Xunit;

namespace Vexora.Application.Test.Platform;

public sealed class StatementPdfGeneratorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(500)]
    public void Generate_returns_pdf_bytes_for_row_counts(int count)
    {
        var generator = new QuestPdfStatementGenerator();
        var statement = CreateStatement(count);
        var bytes = generator.Generate(statement);
        Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
    }

    [Fact]
    public void Generate_handles_long_names_mixed_text_and_large_amounts()
    {
        var generator = new QuestPdfStatementGenerator();
        var statement = CreateStatement(1) with
        {
            CustomerName = new string('م', 300) + " Customer ABC",
            Totals = new StatementTotalsDto(999999999999m, 1000000000m, 2000000000m, 3000000000m, 1001999999999m, 1, 1),
            Rows = new[]
            {
                new StatementRowDto("Payment", 1, new DateTime(2026, 10, 25), "۱۴۰۵/۰۷/۰۳", 999999999999m, null, null, null, null, null)
            }
        };
        var bytes = generator.Generate(statement);
        Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
    }

    private static CustomerStatementDto CreateStatement(int count)
    {
        var rows = Enumerable.Range(1, count)
            .Select(index => new StatementRowDto("Check", index, new DateTime(2026, 10, 25), "۱۴۰۵/۰۷/۰۳", 500000m,
                "Pending", $"CHK-{index}", new DateTime(2026, 10, 25).Date, "۱۴۰۵/۰۷/۱۰", "Registered"))
            .ToArray();
        return new CustomerStatementDto(1, "مشتری آزمایشی", null, null,
            new StatementTotalsDto(100000m, 500000m, 0m, 0m, 100000m, 1, count),
            rows, new DateTimeOffset(2026, 10, 25, 12, 0, 0, TimeSpan.Zero));
    }
}
