using System;
using System.Collections.Generic;
using System.Text;

namespace Vexora.Application.NLU.Models.Intents;

public enum IntentType
{
    Unknown = 0,

    // Commands
    RegisterSale = 1,
    RegisterPayment = 2,
    RegisterDebt = 3,
    RegisterCheck = 4,

    // Customer Queries
    GetCustomerBalance = 10,
    GetCustomerTransactions = 11,
    GetCustomerInfo = 12,

    // Reports / Queries
    GetTodaySales = 20,
    GetTodayPayments = 21,
    GetUpcomingChecks = 22,
    GetUpcomingReceivables = 23
}
