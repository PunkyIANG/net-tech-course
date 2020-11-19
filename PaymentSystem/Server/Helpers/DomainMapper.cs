using PaymentSystem.Server.Models;
using PaymentSystem.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Helpers
{
    public static class DomainMapper
    {
        public static TransactionDto ToDto(Transactions transaction)
        {
            return transaction == null
                ? null
                : new TransactionDto
                {
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    DestinationWalletId = transaction.DestinationWalletId,
                    SourceWalletId = transaction.SourceWalletId,
                    Date = transaction.Date
                };
        }
    }
}
