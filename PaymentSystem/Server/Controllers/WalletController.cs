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
        public async Task<IActionResult> DeleteWallet([FromRoute] Guid id)
        {
            var deleteWalletCommand = new DeleteWalletCommand
            {
                UserId = userManager.GetUserId(User),
                WalletId = id
            };

            var deleteWalletResult = await mediator.Send(deleteWalletCommand);

            if (!deleteWalletResult.IsSuccessful)
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}
