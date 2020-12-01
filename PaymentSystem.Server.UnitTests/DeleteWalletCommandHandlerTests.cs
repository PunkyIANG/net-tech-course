using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PaymentSystem.Server.Application.Wallets.Commands;
using PaymentSystem.Server.Data;
using PaymentSystem.Server.Models;

namespace PaymentSystem.Server.UnitTests
{
    public class DeleteWalletCommandHandlerTests
    {
        private ApplicationDbContext context;
        private Guid walletGuid;

        [SetUp]
        public void Setup()
        {
            context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite("filename=Test.db")
                .Options, Microsoft.Extensions.Options.Options.Create(new OperationalStoreOptions()));

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            walletGuid = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = "test_user_id",
                Wallets = new List<Wallet>
                {
                    new Wallet
                    {
                        Id = walletGuid,
                        Amount = 100,
                        Currency = "EC"
                    }
                }
            };

            context.Add(user);

            context.SaveChanges();
        }

        [Test]
        public async Task DeleteWalletSuccessful()
        {
            var sut = new DeleteWalletCommandHandler(context);

            var command = new DeleteWalletCommand
            {
                UserId = "test_user_id",
                WalletId = walletGuid
            };

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.IsSuccessful);
        }

        [Test]
        public async Task DeleteWalletMissing()
        {
            var sut = new DeleteWalletCommandHandler(context);

            var command = new DeleteWalletCommand
            {
                UserId = "test_user_id",
                WalletId = Guid.NewGuid()
            };

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccessful);
                Assert.AreEqual("WALLET_MISSING_OR_UNAUTHORIZED", result.FailureReason);
            }
            );
        }

        [Test]
        public async Task DeleteWalletUnauthorizedUser()
        {
            var sut = new DeleteWalletCommandHandler(context);

            var command = new DeleteWalletCommand
            {
                UserId = "test_unauthorized_user_id",
                WalletId = walletGuid
            };

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccessful);
                Assert.AreEqual("WALLET_MISSING_OR_UNAUTHORIZED", result.FailureReason);
            }
            );
        }
    }
}
