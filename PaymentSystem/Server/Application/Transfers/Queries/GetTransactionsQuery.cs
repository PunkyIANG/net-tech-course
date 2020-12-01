using MediatR;
using PaymentSystem.Server.Data;
using PaymentSystem.Server.Helpers;
using PaymentSystem.Server.Models;
using PaymentSystem.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Application.Transfers.Queries
{
    public class GetTransactionsQuery : IRequest<TransactionsHistoryData>
    {
        public string UserId { get; set; }
        public int ItemsPerPage { get; set; }
        public int PageNumber { get; set; }
        public Direction Direction { get; set; }
    }

    public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, TransactionsHistoryData>
    {
        private readonly ApplicationDbContext context;

        public GetTransactionsQueryHandler(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<TransactionsHistoryData> Handle(GetTransactionsQuery query, CancellationToken cancellationToken)
        {
            var walletIds = context.Wallets.Where(w => w.ApplicationUserId == query.UserId).Select(w => w.Id).ToList();

            IQueryable<Transactions> dbQuery;
            Transactions[] transactions;

            switch (query.Direction)
            {
                case Direction.Inbound:
                    dbQuery = context.Transactions.Where(t => walletIds.Contains(t.SourceWalletId));
                    transactions = dbQuery.OrderByDescending(x => x.Date)
                        .Skip((query.PageNumber - 1) * query.ItemsPerPage).Take(query.ItemsPerPage).ToArray();
                    break;

                case Direction.Outbound:
                    dbQuery = context.Transactions.Where(t => walletIds.Contains(t.DestinationWalletId));
                    transactions = dbQuery.OrderByDescending(x => x.Date)
                        .Skip((query.PageNumber - 1) * query.ItemsPerPage).Take(query.ItemsPerPage).ToArray();
                    break;
                case Direction.None:
                default:
                    dbQuery = context.Transactions.Where(t =>
                        walletIds.Contains(t.DestinationWalletId) || walletIds.Contains(t.SourceWalletId));
                    transactions = dbQuery.OrderByDescending(x => x.Date)
                        .Skip((query.PageNumber - 1) * query.ItemsPerPage).Take(query.ItemsPerPage).ToArray();
                    break;
            }

            var transactionsData = new TransactionsHistoryData
            {
                Transactions = transactions.Select(DomainMapper.ToDto).ToArray(),
                ItemCount = dbQuery.Count()
            };

            return transactionsData;
        }
    }
}
