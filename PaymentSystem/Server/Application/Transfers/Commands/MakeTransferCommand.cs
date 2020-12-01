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
    public class MakeTransferCommand : IRequest<MakeTransferResult>
    {
        public string UserId { get; set; }
        public TransferDto Data { get; set; }
    }

    public class MakeTransferResult 
    {
        public bool IsSuccessful { get; set; }
        public string SuccessMessage { get; set; }
        public string FailureReason { get; set; }

        public static MakeTransferResult ReturnSuccess()
        {
            return new MakeTransferResult { IsSuccessful = true };
        }

        public static MakeTransferResult ReturnSuccess(string successMessage)
        {
            return new MakeTransferResult {
                IsSuccessful = true,
                SuccessMessage = successMessage
            };
        }


        public static MakeTransferResult ReturnFailure(string failureReason)
        {
            return new MakeTransferResult
            {
                IsSuccessful = false,
                FailureReason = failureReason
            };
        }

    }

    public class MakeTransferCommandHandler : IRequestHandler<MakeTransferCommand, MakeTransferResult>
    {
        private readonly ApplicationDbContext context;
        public MakeTransferCommandHandler(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<MakeTransferResult> Handle(MakeTransferCommand command, CancellationToken cancellationToken)
        {
            var user = await context.Users.Include(x => x.Wallets).FirstOrDefaultAsync(x => x.Id == command.UserId);

            if (!user.Wallets.Any(x => x.Currency == command.Data.Currency))
            {
                return MakeTransferResult.ReturnFailure("MISSING_SOURCE_WALLET");
            }

            var source = user.Wallets.FirstOrDefault(x => x.Currency == command.Data.Currency);

            var destinationUser = context.Users.Include(x => x.Wallets).FirstOrDefault(x => x.UserName == command.Data.Username);

            if (destinationUser == null)
            {
                return MakeTransferResult.ReturnFailure("MISSING_DESTINATION_USER");
            }

            var destination = destinationUser.Wallets.FirstOrDefault(x => x.Currency == command.Data.Currency);

            if (source.Amount < command.Data.Amount)
            {
                return MakeTransferResult.ReturnFailure("INSUFFICIENT_FUNDS");
            }

            var successMessage = string.Empty;
            if (destination == null)
            {
                destination = new Models.Wallet
                {
                    Amount = 0,
                    Currency = command.Data.Currency
                };

                destinationUser.Wallets.Add(destination);
                successMessage = "CREATED_NEW_WALLET";
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

            return MakeTransferResult.ReturnSuccess(successMessage);
        }
    }
}
