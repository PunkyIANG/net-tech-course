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
    public class DeleteWalletCommand : IRequest<BoolResult>
    {
        public string UserId { get; set; }
        public Guid WalletId { get; set; }
    }

    public class DeleteWalletCommandHandler : IRequestHandler<DeleteWalletCommand, BoolResult>
    {
        private readonly ApplicationDbContext context;
        public DeleteWalletCommandHandler(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<BoolResult> Handle(DeleteWalletCommand command, CancellationToken cancellationToken)
        {
            var user = await context.Users.Include(x => x.Wallets).FirstOrDefaultAsync(x => x.Id == command.UserId);

            var wallet = user.Wallets.Find(x => x.Id == command.WalletId);

            if (wallet != null)
            {
                user.Wallets.Remove(wallet);
            }
            else
            {
                return BoolResult.ReturnFailure();
            }

            context.SaveChanges();

            return BoolResult.ReturnSuccess();
        }
    }
}
