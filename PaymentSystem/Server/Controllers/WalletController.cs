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

        [HttpPost]
        public IActionResult CreateWallet([FromQuery]string currency)
        {
            var userId = userManager.GetUserId(User);

            var wallet = new Wallet
            {
                Amount = 0,
                Currency = currency
            };

            var user = context.Users.Include(x => x.Wallets).FirstOrDefault(x => x.Id == userId);

            if (user.Wallets == null)
            {
                user.Wallets = new List<Wallet> { wallet };
            }

            user.Wallets.Add(wallet);

            context.SaveChanges();

            return Ok();
        }

        [HttpDelete]
        public IActionResult DeleteWallet([FromQuery] Guid id)
        {
            var userId = userManager.GetUserId(User);

            var user = context.Users.Include(x => x.Wallets).FirstOrDefault(x => x.Id == userId);

            var walletIndex = user.Wallets.FindIndex(x => x.Id == id);

            if (walletIndex >= 0)    //if the user has a wallet with this id
            {
                user.Wallets.RemoveAt(walletIndex);
            }
            else
            {
                return BadRequest();
            }

            context.SaveChanges();

            return Ok();
        }
    }
}
