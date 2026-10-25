using Vexora.Application.Ledger.Models;

namespace Vexora.Application.Ledger.Customers;

public static class CustomerSummaryBuilder
{
    private const string PersianLetterOrder = "ابپتثجچحخدذرزژسشصضطظعغفقکگلمنوهی";

    public static IReadOnlyList<CustomerSummaryDto> Build(
        IEnumerable<CustomerRef> customers,
        IEnumerable<PaymentAggregate> payments,
        IEnumerable<CheckAggregate> checks)
    {
        var paymentsByCustomer = payments.ToDictionary(x => x.CustomerId);
        var checksByCustomer = checks.ToDictionary(x => x.CustomerId);

        var result = new List<CustomerSummaryDto>();

        foreach (var customer in customers)
        {
            paymentsByCustomer.TryGetValue(customer.Id, out var payment);
            checksByCustomer.TryGetValue(customer.Id, out var check);

            var cash = payment?.Total ?? 0m;
            var paid = check?.PaidTotal ?? 0m;

            result.Add(new CustomerSummaryDto(
                customer.Id,
                customer.FullName,
                cash,
                check?.PendingTotal ?? 0m,
                check?.PendingCount ?? 0,
                check?.OverdueCount ?? 0,
                cash + paid,
                LatestOf(payment?.LastAt, check?.LastAt)));
        }

        return result;
    }

    public static IReadOnlyList<CustomerSummaryDto> Sort(
        IEnumerable<CustomerSummaryDto> summaries,
        CustomerSort sort)
        => sort == CustomerSort.Name
            ? summaries
                .OrderBy(x => NameSortKey(x.FullName), StringComparer.Ordinal)
                .ThenBy(x => x.Id)
                .ToList()
            : summaries
                .OrderByDescending(x => x.LastActivityAt ?? DateTimeOffset.MinValue)
                .ThenBy(x => NameSortKey(x.FullName), StringComparer.Ordinal)
                .ThenBy(x => x.Id)
                .ToList();

    public static string NameSortKey(string name)
    {
        var chars = name.Trim().ToCharArray();

        for (var i = 0; i < chars.Length; i++)
        {
            var character = chars[i] switch
            {
                'ي' or 'ى' => 'ی',
                'ك' => 'ک',
                'آ' or 'أ' or 'إ' => 'ا',
                '\u200c' => ' ',
                var other => other
            };

            var index = PersianLetterOrder.IndexOf(character);
            chars[i] = index >= 0 ? (char)(0x0100 + index) : character;
        }

        return new string(chars);
    }

    private static DateTimeOffset? LatestOf(DateTimeOffset? first, DateTimeOffset? second)
    {
        if (first is null)
            return second;

        if (second is null)
            return first;

        return first > second ? first : second;
    }
}
