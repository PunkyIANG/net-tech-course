using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentSystem.Server.Data;
using PaymentSystem.Server.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Application.Wallets.Commands
{
    public class DeleteWalletCommand : IRequest<DeleteWalletResult>
    {
        public string UserId { get; set; }
        public Guid WalletId { get; set; }
    }

    public class DeleteWalletResult  
    {
        public bool IsSuccessful { get; set; }
        public string SuccessMessage { get; set; }
        public string FailureReason { get; set; }

        public static DeleteWalletResult ReturnSuccess()
        {
            return new DeleteWalletResult { IsSuccessful = true };
        }

        public static DeleteWalletResult ReturnFailure(string failureReason)
        {
            return new DeleteWalletResult
            {
                IsSuccessful = false,
                FailureReason = failureReason
            };
        }
    }


public class DeleteWalletCommandHandler : IRequestHandler<DeleteWalletCommand, DeleteWalletResult>
    {
        private readonly ApplicationDbContext context;
        public DeleteWalletCommandHandler(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<DeleteWalletResult> Handle(DeleteWalletCommand command, CancellationToken cancellationToken)
        {
            //var user = await context.Users.Include(x => x.Wallets).FirstOrDefaultAsync(x => x.Id == command.UserId);

            //var wallet = user.Wallets.Find(x => x.Id == command.WalletId);

            var wallet = context.Wallets.FirstOrDefault(w => w.Id == command.WalletId && w.ApplicationUserId == command.UserId);

            if (wallet != null)
            {
                context.Wallets.Remove(wallet);
            }
            else
            {
                return DeleteWalletResult.ReturnFailure("WALLET_MISSING_OR_UNAUTHORIZED");
            }

            context.SaveChanges();

            return DeleteWalletResult.ReturnSuccess();
        }
    }
}
