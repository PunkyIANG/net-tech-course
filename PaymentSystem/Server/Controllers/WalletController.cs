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

namespace PaymentSystem.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public WalletController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public List<Wallet> GetWallets()
        {
            var userId = userManager.GetUserId(User);
            var wallets = context.Users.Include(x => x.Wallets).FirstOrDefault(x => x.Id == userId).Wallets;
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
        public IActionResult CreateWallet([FromQuery] string currency)
        {
            if (!CurrencyManager.Currencies.Contains(currency))
            {
                return BadRequest();
            }

            var userId = userManager.GetUserId(User);

            var user = context.Users.Include(x => x.Wallets).FirstOrDefault(x => x.Id == userId);

            if (user.Wallets.Any(x => x.Currency == currency))
            {
                return BadRequest();
            }

            var wallet = new Wallet
            {
                Amount = 0,
                Currency = currency
            };


            if (user.Wallets == null)
            {
                user.Wallets = new List<Wallet> { wallet };
            }

            user.Wallets.Add(wallet);

            context.SaveChanges();

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
                    query = context.Transactions.Where(t => walletIds.Contains(t.SourceWalletId));
                    transactions = query.OrderByDescending(x => x.Date)
                        .Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage).ToArray();
                    break;

                case Direction.Outbound:
                    query = context.Transactions.Where(t => walletIds.Contains(t.DestinationWalletId));
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

            var transactionsData = new TransactionsHistoryData
            {
                Transactions = transactions.Select(DomainMapper.ToDto).ToArray(),
                ItemCount = query.Count()
            };

            return transactionsData;
        }
    }
}
