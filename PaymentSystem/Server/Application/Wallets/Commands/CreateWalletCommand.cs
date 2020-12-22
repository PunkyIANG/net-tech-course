using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentSystem.Server.Application.Currencies;
using PaymentSystem.Server.Application.Promotion;
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
    public class CreateWalletCommand : IRequest<CreateWalletResult>
    {
        public string UserId { get; set; }
        public string Currency { get; set; }
    }

    public class CreateWalletResult 
    {
        public bool IsSuccessful { get; set; }
        public ExecutionMessage CurrentExecutionMessage { get; set; }
        public decimal Amount { get; set; }
        public enum ExecutionMessage
        {
            Success,
            ErrorInvalidCurrency,
            ErrorWalletAlreadyExists
        }

        public static CreateWalletResult ReturnSuccess(ExecutionMessage currentExecutionMessage, decimal amount)
        {
            return new CreateWalletResult { 
                IsSuccessful = true,
                Amount = amount,
                CurrentExecutionMessage = currentExecutionMessage
            };
        }

        public static CreateWalletResult ReturnFailure(ExecutionMessage currentExecutionMessage)
        {
            return new CreateWalletResult
            {
                IsSuccessful = false,
                CurrentExecutionMessage = currentExecutionMessage
            };
        }
    }

public class CreateWalletCommandHandler : IRequestHandler<CreateWalletCommand, CreateWalletResult>
    {
        private readonly ApplicationDbContext context;
        private readonly IPromotionManager promotionManager;
        private readonly ICurrencyManager currencyManager;

        public CreateWalletCommandHandler(ApplicationDbContext context, IPromotionManager promotionManager, ICurrencyManager currencyManager)
        {
            this.context = context;
            this.promotionManager = promotionManager;
            this.currencyManager = currencyManager;
        }

        public async Task<CreateWalletResult> Handle(CreateWalletCommand command, CancellationToken cancellationToken)
        {
            if (!currencyManager.GetCurrencies().Contains(command.Currency))
            {
                return CreateWalletResult.ReturnFailure(CreateWalletResult.ExecutionMessage.ErrorInvalidCurrency);
            }

            var user = await context.Users.Include(x => x.Wallets).FirstOrDefaultAsync(x => x.Id == command.UserId);

            if (user.Wallets.Any(x => x.Currency == command.Currency))
            {
                return CreateWalletResult.ReturnFailure(CreateWalletResult.ExecutionMessage.ErrorWalletAlreadyExists);
            }

            var wallet = new Wallet
            {
                Amount = promotionManager.GetDefaultAmount(command.Currency),
                Currency = command.Currency
            };


            if (user.Wallets == null)
            {
                user.Wallets = new List<Wallet>();
            }

            user.Wallets.Add(wallet);

            context.SaveChanges();

            return CreateWalletResult.ReturnSuccess(CreateWalletResult.ExecutionMessage.Success, wallet.Amount);

        }
    }
}
