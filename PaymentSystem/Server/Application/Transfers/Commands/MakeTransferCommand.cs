using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentSystem.Server.Data;
using PaymentSystem.Server.Helpers;
using PaymentSystem.Server.Models;
using PaymentSystem.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Application.Transfers.Commands
{
    public class MakeTransferCommand : IRequest<BoolResult>
    {
        public string UserId { get; set; }
        public TransferDto Data { get; set; }
    }

    public class MakeTransferCommandHandler : IRequestHandler<MakeTransferCommand, BoolResult>
    {
        private readonly ApplicationDbContext context;
        public MakeTransferCommandHandler(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<BoolResult> Handle(MakeTransferCommand command, CancellationToken cancellationToken)
        {
            var user = await context.Users.Include(x => x.Wallets).FirstOrDefaultAsync(x => x.Id == command.UserId);

            if (!user.Wallets.Any(x => x.Currency == command.Data.Currency))
            {
                return BoolResult.ReturnFailure();
            }

            var source = user.Wallets.FirstOrDefault(x => x.Currency == command.Data.Currency);

            var destinationUser = context.Users.Include(x => x.Wallets).FirstOrDefault(x => x.UserName == command.Data.Username);

            if (destinationUser == null)
            {
                return BoolResult.ReturnFailure();
            }

            var destination = destinationUser.Wallets.FirstOrDefault(x => x.Currency == command.Data.Currency);

            if (source.Amount < command.Data.Amount)
            {
                return BoolResult.ReturnFailure();
            }

            if (destination == null)
            {
                destination = new Models.Wallet
                {
                    Amount = 0,
                    Currency = command.Data.Currency
                };

                destinationUser.Wallets.Add(destination);
            }

            source.Amount -= command.Data.Amount;
            destination.Amount += command.Data.Amount;

            var transaction = new Transactions()
            {
                Amount = command.Data.Amount,
                Date = DateTime.Now,
                SourceWalletId = source.Id,
                DestinationWalletId = destination.Id
            };

            context.Add(transaction);

            context.SaveChanges();

            return BoolResult.ReturnSuccess();
        }
    }
}
