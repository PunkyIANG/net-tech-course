using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentSystem.Server.Data;
using PaymentSystem.Server.Helpers;
using PaymentSystem.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Application.Wallets.Commands
{
    public class CreateWalletCommand : IRequest<BoolResult>
    {
        public string UserId { get; set; }
        public string Currency { get; set; }
    }

    public class CreateWalletCommandHandler : IRequestHandler<CreateWalletCommand, BoolResult>
    {
        private readonly ApplicationDbContext context;
        public CreateWalletCommandHandler(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<BoolResult> Handle(CreateWalletCommand command, CancellationToken cancellationToken)
        {
            if (!CurrencyManager.Currencies.Contains(command.Currency))
            {
                return BoolResult.ReturnFailure();
            }

            var user = await context.Users.Include(x => x.Wallets).FirstOrDefaultAsync(x => x.Id == command.UserId);

            if (user.Wallets.Any(x => x.Currency == command.Currency))
            {
                return BoolResult.ReturnFailure();
            }

            var wallet = new Wallet
            {
                Amount = 0,
                Currency = command.Currency
            };


            if (user.Wallets == null)
            {
                user.Wallets = new List<Wallet>();
            }

            user.Wallets.Add(wallet);

            context.SaveChanges();

            return BoolResult.ReturnSuccess();

        }
    }
}
