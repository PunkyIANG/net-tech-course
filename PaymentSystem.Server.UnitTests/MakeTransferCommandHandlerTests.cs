using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PaymentSystem.Server.Application.Transfers.Commands;
using PaymentSystem.Server.Data;
using PaymentSystem.Server.Models;

namespace PaymentSystem.Server.UnitTests
{
    class MakeTransferCommandHandlerTests
    {
        private ApplicationDbContext context;

        [SetUp]
        public void Setup()
        {
            context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite("filename=Test.db")
                .Options, Microsoft.Extensions.Options.Options.Create(new OperationalStoreOptions()));

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var sourceUser = new ApplicationUser
            {
                Id = "test_source_user_id",
                Wallets = new List<Wallet>
                {
                    new Wallet
                    {
                        Amount = 100,
                        Currency = "EC"
                    },

                    new Wallet
                    {
                        Amount = 0,
                        Currency = "EUR"
                    },

                    new Wallet
                    {
                        Amount = 100,
                        Currency = "BTC"
                    }
                }
            };

            var destinationUser = new ApplicationUser
            {
                Id = "test_dest_user_id",
                UserName = "test_dest_username",
                Wallets = new List<Wallet>
                {
                    new Wallet
                    {
                        Amount = 0,
                        Currency = "EC"
                    }
                }
            };

            context.Add(sourceUser);
            context.Add(destinationUser);

            context.SaveChanges();
        }

        [Test] 
        public async Task MakeTransferSuccessful()
        {
            var sut = new MakeTransferCommandHandler(context);

            var command = new MakeTransferCommand
            {
                UserId = "test_source_user_id",
                Data = new Shared.TransferDto
                {
                    Username = "test_dest_username",
                    Amount = 100,
                    Currency = "EC"
                }
            };

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.IsSuccessful);
        }

        [Test]
        public async Task MakeTransferSuccessfulNewWallet()
        {
            var sut = new MakeTransferCommandHandler(context);

            var command = new MakeTransferCommand
            {
                UserId = "test_source_user_id",
                Data = new Shared.TransferDto
                {
                    Username = "test_dest_username",
                    Amount = 100,
                    Currency = "BTC"
                }
            };

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(result.IsSuccessful);
                Assert.AreEqual("CREATED_NEW_WALLET", result.SuccessMessage);
            });
        }



        [Test]
        public async Task MakeTransferMissingSourceWallet()
        {
            var sut = new MakeTransferCommandHandler(context);

            var command = new MakeTransferCommand
            {
                UserId = "test_source_user_id",
                Data = new Shared.TransferDto
                {
                    Username = "test_dest_username",
                    Amount = 100,
                    Currency = "USD"
                }
            };

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccessful);
                Assert.AreEqual("MISSING_SOURCE_WALLET", result.FailureReason);
            });
        }

        [Test]
        public async Task MakeTransferMissingDestinationUser()
        {
            var sut = new MakeTransferCommandHandler(context);

            var command = new MakeTransferCommand
            {
                UserId = "test_source_user_id",
                Data = new Shared.TransferDto
                {
                    Username = "test_fake_dest_username",
                    Amount = 100,
                    Currency = "EC"
                }
            };

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccessful);
                Assert.AreEqual("MISSING_DESTINATION_USER", result.FailureReason);
            });
        }

        [Test]
        public async Task MakeTransferInsufficientFunds()
        {
            var sut = new MakeTransferCommandHandler(context);

            var command = new MakeTransferCommand
            {
                UserId = "test_source_user_id",
                Data = new Shared.TransferDto
                {
                    Username = "test_dest_username",
                    Amount = 100,
                    Currency = "EUR"
                }
            };

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccessful);
                Assert.AreEqual("INSUFFICIENT_FUNDS", result.FailureReason);
            });
        }
    }
}
