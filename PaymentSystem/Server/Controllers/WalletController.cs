using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PaymentSystem.Server.Data;
using PaymentSystem.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PaymentSystem.Shared;
using Wallet = PaymentSystem.Server.Models.Wallet;
using PaymentSystem.Server.Helpers;
using PaymentSystem.Server.Application.Wallets.Queries;
using MediatR;
using PaymentSystem.Server.Application.Wallets.Commands;

namespace PaymentSystem.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMediator mediator;

        public WalletController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMediator mediator)
        {
            this.context = context;
            this.userManager = userManager;
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<List<Wallet>> GetWallets()
        {
            var query = new GetWalletsQuery
            {
                UserId = userManager.GetUserId(User)
            };
            var wallets = await mediator.Send(query);
            return wallets;
        }

        [HttpGet]
        [Route("{id}")]
        public Wallet GetWallet(Guid id)
        {
            var userId = userManager.GetUserId(User);
            var wallet = context.Users.Include(x => x.Wallets).FirstOrDefault(x => x.Id == userId).Wallets.FirstOrDefault(x => x.Id == id);
            return wallet;
        }


        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromQuery] string currency)
        {
            var createWalletCommand = new CreateWalletCommand
            {
                UserId = userManager.GetUserId(User),
                Currency = currency
            };

            var createWalletResult = await mediator.Send(createWalletCommand);

            if (!createWalletResult.IsSuccessful)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult DeleteWallet([FromRoute] Guid id)
        {
            var userId = userManager.GetUserId(User);
            var user = context.Users.Include(x => x.Wallets).FirstOrDefault(x => x.Id == userId);

            var wallet = user.Wallets.Find(x => x.Id == id);

            if (wallet != null)    //if the user has a wallet with this id
            {
                user.Wallets.Remove(wallet);
            }
            else
            {
                return BadRequest();
            }

            context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        [Route("transfer")]
        public IActionResult MakeTransfer([FromBody] TransferDto data)
        {
            var userId = userManager.GetUserId(User);
            var user = context.Users.Include(x => x.Wallets).FirstOrDefault(x => x.Id == userId);

            if (!user.Wallets.Any(x => x.Currency == data.Currency))
            {
                return BadRequest();
            }

            var source = user.Wallets.FirstOrDefault(x => x.Currency == data.Currency);

            var destinationUser = context.Users.Include(x => x.Wallets).FirstOrDefault(x => x.UserName == data.Username);

            if (destinationUser == null)
            {
                return BadRequest();
            }

            var destination = destinationUser.Wallets.FirstOrDefault(x => x.Currency == data.Currency);

            if (source.Amount < data.Amount)
            {
                return BadRequest();
            }

            if (destination == null)
            {
                destination = new Wallet
                {
                    Amount = 0,
                    Currency = data.Currency
                };

                destinationUser.Wallets.Add(destination);
            }

            source.Amount -= data.Amount;
            destination.Amount += data.Amount;

            var transaction = new Transactions()
            {
                Amount = data.Amount,
                Date = DateTime.Now,
                SourceWalletId = source.Id,
                DestinationWalletId = destination.Id
            };

            context.Add(transaction);

            context.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [Route("transfers/{itemsPerPage}/{pageNumber}")]
        public TransactionsHistoryData GetTransactions(int itemsPerPage, int pageNumber, [FromQuery] Direction direction)
        {
            var userId = userManager.GetUserId(User);

            var walletIds = context.Wallets.Where(w => w.ApplicationUserId == userId).Select(w => w.Id).ToList();

            IQueryable<Transactions> query;
            Transactions[] transactions;

            switch (direction)
            {
                case Direction.Inbound:
                    query = context.Transactions.Where(t => walletIds.Contains(t.DestinationWalletId));
                    transactions = query.OrderByDescending(x => x.Date)
                        .Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage).ToArray();
                    break;

                case Direction.Outbound:
                    query = context.Transactions.Where(t => walletIds.Contains(t.SourceWalletId));
                    transactions = query.OrderByDescending(x => x.Date)
                        .Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage).ToArray();
                    break;
                case Direction.None:
                default:
                    query = context.Transactions.Where(t =>
                        walletIds.Contains(t.DestinationWalletId) || walletIds.Contains(t.SourceWalletId));
                    transactions = query.OrderByDescending(x => x.Date)
                        .Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage).ToArray();
                    break;
            }

            //incredibly rough, need better ideas for getting the usernames and currency out of the transaction
            var transactionsData = new TransactionsHistoryData  
            {
                Transactions = transactions.Select(t => DomainMapper.ToDto(t, 
                    context.Users.Find(context.Wallets.Find(t.SourceWalletId).ApplicationUserId),
                    context.Users.Find(context.Wallets.Find(t.DestinationWalletId).ApplicationUserId),
                    context.Wallets.Find(t.SourceWalletId)
                )).ToArray(),
                ItemCount = query.Count()
            };

            return transactionsData;
        }
    }
}
