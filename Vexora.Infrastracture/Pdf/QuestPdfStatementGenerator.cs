using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Drawing;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vexora.Application.Exceptions;
using Vexora.Application.Platform.Statements;

namespace Vexora.Infrastracture.Pdf;

public sealed class QuestPdfStatementGenerator : IStatementPdfGenerator
{
    private static bool HasVazirmatn;
    static QuestPdfStatementGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var fonts = Path.Combine(AppContext.BaseDirectory, "Pdf", "Fonts");
        var regular = Path.Combine(fonts, "Vazirmatn-Regular.ttf");
        var bold = Path.Combine(fonts, "Vazirmatn-Bold.ttf");
        if (File.Exists(regular) && File.Exists(bold))
        {
            using var regularStream = File.OpenRead(regular);
            using var boldStream = File.OpenRead(bold);
            FontManager.RegisterFont(regularStream);
            FontManager.RegisterFont(boldStream);
            HasVazirmatn = true;
        }
    }

    public byte[] Generate(CustomerStatementDto statement)
    {
        if (statement.Rows.Count > 5000)
            throw new StatementTooLargeException();

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.ContentFromRightToLeft();
                page.Margin(24);
                page.DefaultTextStyle(style => (HasVazirmatn ? style.FontFamily("Vazirmatn") : style).FontSize(9));
                page.Header().Column(column =>
                {
                    column.Item().AlignRight().Text("نوتا").FontSize(18).Bold();
                    column.Item().AlignRight().Text($"حساب مشتری: {statement.CustomerName}");
                    column.Item().AlignRight().Text($"بازه: {FormatDate(statement.From)} تا {FormatDate(statement.To)}");
                    column.Item().AlignRight().Text($"تاریخ تولید: {PersianDate(statement.GeneratedAt)}");
                });
                page.Content().Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });
                        table.Cell().Border(1).Padding(4).Text("دریافتی نقدی");
                        table.Cell().Border(1).Padding(4).Text("چک در انتظار");
                        table.Cell().Border(1).Padding(4).Text("چک وصول‌شده");
                        table.Cell().Border(1).Padding(4).Text("چک برگشتی");
                        table.Cell().Border(1).Padding(4).Text("جمع دریافتی");
                        table.Cell().Border(1).Padding(4).Text(Amount(statement.Totals.CashReceived));
                        table.Cell().Border(1).Padding(4).Text(Amount(statement.Totals.ChecksPending));
                        table.Cell().Border(1).Padding(4).Text(Amount(statement.Totals.ChecksPaid));
                        table.Cell().Border(1).Padding(4).Text(Amount(statement.Totals.ChecksBounced));
                        table.Cell().Border(1).Padding(4).Text(Amount(statement.Totals.TotalReceived));
                    });
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });
                        table.Header(header =>
                        {
                            foreach (var label in new[] { "نوع", "تاریخ", "مبلغ", "وضعیت", "سررسید", "شماره چک", "صیاد" })
                                header.Cell().Background(Colors.Grey.Lighten2).Border(1).Padding(4).Text(label).Bold();
                        });
                        foreach (var row in statement.Rows)
                        {
                            table.Cell().BorderBottom(0.5f).Padding(4).Text(row.Type == "Payment" ? "پرداخت" : "چک");
                            table.Cell().BorderBottom(0.5f).Padding(4).Text(row.DatePersian);
                            table.Cell().BorderBottom(0.5f).Padding(4).Text(Amount(row.Amount));
                            table.Cell().BorderBottom(0.5f).Padding(4).Text(Status(row.Status));
                            table.Cell().BorderBottom(0.5f).Padding(4).Text(row.DueDatePersian ?? "-");
                            table.Cell().BorderBottom(0.5f).Padding(4).Text(row.CheckNumber ?? "-");
                            table.Cell().BorderBottom(0.5f).Padding(4).Text(BankRegistration(row.BankRegistration));
                        }
                    });
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("صفحه ");
                    text.CurrentPageNumber();
                    text.Span(" از ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static string Amount(decimal value) => string.Concat(value.ToString("#,##0", CultureInfo.InvariantCulture)
        .Select(ch => ch == ',' ? '٬' : ch is >= '0' and <= '9' ? (char)('۰' + ch - '0') : ch));

    private static string Status(string? status) => status switch
    {
        "Pending" => "در انتظار",
        "Paid" => "وصول‌شده",
        "Bounced" => "برگشتی",
        null => "—",
        _ => status
    };

    private static string BankRegistration(string? status) => status switch
    {
        "Registered" => "ثبت‌شده",
        "NotRegistered" => "ثبت‌نشده",
        null => "—",
        _ => status
    };

    private static string FormatDate(DateTime? date) => date is null ? "—" : PersianDate(new DateTimeOffset(date.Value, TimeSpan.Zero));

    private static string PersianDate(DateTimeOffset date)
    {
        var calendar = new PersianCalendar();
        return $"{calendar.GetYear(date.DateTime):0000}/{calendar.GetMonth(date.DateTime):00}/{calendar.GetDayOfMonth(date.DateTime):00}";
    }
}

public sealed class StatementTooLargeException() : AppException(
    "STATEMENT_TOO_LARGE", "گزارش بیش از ۵۰۰۰ ردیف دارد.", AppErrorType.Validation);
