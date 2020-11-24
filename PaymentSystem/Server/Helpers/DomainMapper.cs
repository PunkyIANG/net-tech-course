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
        public static TransactionDto ToDto(Transactions transaction, ApplicationUser sourceUser, ApplicationUser destinationUser, Models.Wallet wallet)
        {
            return transaction == null
                ? null
                : new TransactionDto
                {
                    SourceUsername = sourceUser.UserName,
                    DestinationUsername = destinationUser.UserName,
                    Currency = wallet.Currency,
                    Amount = transaction.Amount,
                    Date = transaction.Date
                };
        }
    }
}
