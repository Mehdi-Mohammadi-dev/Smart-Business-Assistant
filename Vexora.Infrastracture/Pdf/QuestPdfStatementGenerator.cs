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
    private const int MaxRows = 2000;
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
        ArgumentNullException.ThrowIfNull(statement);
        if (statement.Rows.Count > MaxRows)
            throw new StatementTooLargeException();

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.ContentFromRightToLeft();
                page.MarginHorizontal(28);
                page.MarginVertical(24);
                page.DefaultTextStyle(style => (HasVazirmatn ? style.FontFamily("Vazirmatn") : style).FontSize(9).FontColor(Colors.Grey.Darken4));

                page.Header().Column(header =>
                {
                    header.Spacing(4);
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().AlignRight().Text(string.IsNullOrWhiteSpace(statement.BusinessName) ? "گردش حساب" : statement.BusinessName)
                            .FontSize(17).Bold().FontColor(Colors.Blue.Darken3);
                        row.ConstantItem(100).AlignLeft().Text("گزارش مالی مشتری").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                    header.Item().LineHorizontal(1.5f).LineColor(Colors.Blue.Medium);
                    header.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("مشتری: ").Bold();
                            text.Span(statement.CustomerName);
                        });
                        row.RelativeItem().AlignLeft().Text(text =>
                        {
                            text.Span("تاریخ تهیه: ").Bold();
                            text.Span(PersianDate(statement.GeneratedAt));
                        });
                    });
                    header.Item().Text(text =>
                    {
                        text.Span("بازه گزارش: ").Bold();
                        text.Span($"{FormatDate(statement.From)} تا {FormatDate(statement.To)}");
                    });
                    header.Item().PaddingBottom(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(content =>
                {
                    content.Spacing(12);
                    content.Item().Text("خلاصه گردش حساب").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        foreach (var label in new[] { "دریافتی نقدی", "چک در انتظار", "چک وصول‌شده", "چک برگشتی", "جمع دریافتی" })
                            table.Cell().Background(Colors.Blue.Lighten5).Border(0.5f).BorderColor(Colors.Blue.Lighten2).Padding(6).AlignCenter().Text(label).FontSize(8).Bold();

                        foreach (var amount in new[]
                        {
                            Amount(statement.Totals.CashReceived),
                            Amount(statement.Totals.ChecksPending),
                            Amount(statement.Totals.ChecksPaid),
                            Amount(statement.Totals.ChecksBounced),
                            Amount(statement.Totals.TotalReceived)
                        })
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(7).AlignCenter().Text(amount).FontSize(9).Bold();
                    });

                    content.Item().Row(row =>
                    {
                        row.RelativeItem().Text("جزئیات دریافت‌ها و چک‌ها").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
                        row.ConstantItem(125).AlignLeft().Text($"تعداد ردیف‌ها: {PersianNumber(statement.Rows.Count)}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.8f); // نوع
                            columns.RelativeColumn(1.0f); // تاریخ
                            columns.RelativeColumn(1.2f); // مبلغ
                            columns.RelativeColumn(0.9f); // وضعیت
                            columns.RelativeColumn(1.0f); // سررسید
                            columns.RelativeColumn(1.0f); // شماره چک
                            columns.RelativeColumn(1.0f); // صیاد
                        });

                        table.Header(header =>
                        {
                            foreach (var label in new[] { "نوع", "تاریخ", "مبلغ", "وضعیت", "سررسید", "شماره چک", "شناسه صیاد" })
                                header.Cell().Background(Colors.Blue.Darken2).Border(0.5f).BorderColor(Colors.Blue.Darken2).Padding(5).AlignCenter().Text(label).FontSize(8).FontColor(Colors.White).Bold();
                        });

                        foreach (var row in statement.Rows)
                        {
                            table.Cell().Element(BodyCell).Text(row.Type == "Payment" ? "دریافت" : "چک");
                            table.Cell().Element(BodyCell).Text(row.DatePersian);
                            table.Cell().Element(BodyCell).AlignLeft().Text(Amount(row.Amount));
                            table.Cell().Element(BodyCell).Text(Status(row.Status));
                            table.Cell().Element(BodyCell).Text(row.DueDatePersian ?? "—");
                            table.Cell().Element(BodyCell).Text(string.IsNullOrWhiteSpace(row.CheckNumber) ? "—" : row.CheckNumber);
                            table.Cell().Element(BodyCell).Text(BankRegistration(row.BankRegistration));
                        }
                    });
                });

                page.Footer().PaddingTop(8).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten1).Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("این گزارش بر اساس اطلاعات ثبت‌شده در سامانه تهیه شده است.").FontSize(7).FontColor(Colors.Grey.Darken1);
                    row.ConstantItem(100).AlignLeft().Text(text =>
                    {
                        text.Span("صفحه ");
                        text.CurrentPageNumber();
                        text.Span(" از ");
                        text.TotalPages();
                    });
                });
            });
        }).GeneratePdf();
    }

    private static IContainer BodyCell(IContainer container) =>
        container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(3).AlignCenter();

    private static string Amount(decimal value) => string.Concat(value.ToString("#,##0", CultureInfo.InvariantCulture)
        .Select(ch => ch == ',' ? '٬' : ch is >= '0' and <= '9' ? (char)('۰' + ch - '0') : ch));

    private static string PersianNumber(int value) => PersianDigits(value.ToString(CultureInfo.InvariantCulture));

    private static string PersianDigits(string value) => string.Concat(value
        .Select(ch => ch is >= '0' and <= '9' ? (char)('۰' + ch - '0') : ch));

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

    private static string FormatDate(DateTime? date) => date is null
        ? "—"
        : PersianDate(new DateTimeOffset(DateTime.SpecifyKind(date.Value, DateTimeKind.Unspecified), TimeSpan.Zero));

    private static string PersianDate(DateTimeOffset date)
    {
        var calendar = new PersianCalendar();
        return PersianDigits($"{calendar.GetYear(date.DateTime):0000}/{calendar.GetMonth(date.DateTime):00}/{calendar.GetDayOfMonth(date.DateTime):00}");
    }
}

public sealed class StatementTooLargeException() : AppException(
    "STATEMENT_TOO_LARGE", "گزارش بیش از ۲۰۰۰ ردیف دارد؛ بازه تاریخ را محدودتر کنید.", AppErrorType.Validation);
