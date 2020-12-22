using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PaymentSystem.Server.Application.Currencies;
using PaymentSystem.Server.Application.Promotion;
using PaymentSystem.Server.Application.Wallets.Commands;
using PaymentSystem.Server.Data;
using PaymentSystem.Server.Models;

namespace PaymentSystem.Server.UnitTests
{
    public class CreateWalletCommandHandlerTests
    {
        private ApplicationDbContext context;
        private Mock<IPromotionManager> promotionManagerMock;
        private Mock<ICurrencyManager> currencyManagerMock;

        private CreateWalletCommandHandler sut;

        [SetUp]
        public void Setup()
        {
            context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite("filename=Test.db")
                .Options, Microsoft.Extensions.Options.Options.Create(new OperationalStoreOptions()));

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var user = new ApplicationUser
            {
                Id = "test_user_id",
                Wallets = new List<Wallet>
                {
                    new Wallet
                    {
                        Amount = 100,
                        Currency = "EC"
                    }
                }
            };

            context.Add(user);

            context.SaveChanges();

            promotionManagerMock = new Mock<IPromotionManager>();

            promotionManagerMock.Setup(x => x.GetDefaultAmount(It.IsAny<string>())).Returns(500);

            currencyManagerMock = new Mock<ICurrencyManager>();

            currencyManagerMock.Setup(x => x.GetCurrencies()).Returns(new List<string>
            {
                "USD",
                "EUR",
                "GBP",
                "MDL",
                "BTC",
                "EC"
            });

            sut = new CreateWalletCommandHandler(context, promotionManagerMock.Object, currencyManagerMock.Object);

        }

        [TearDown]

        public void Teardown()
        {
            context.Dispose();
        }

        [Test]
        public async Task CreateWalletSuccessful()
        {
            var command = new CreateWalletCommand
            {
                UserId = "test_user_id",
                Currency = "EUR"
            };

            var result = await sut.Handle(command, CancellationToken.None);
            bool walletCreationSuccess = context.Wallets.Where(w => w.Currency == "EUR" & w.Amount == 0) != null;

            Assert.Multiple(() =>
            {
                Assert.IsTrue(result.IsSuccessful);
                Assert.IsTrue(walletCreationSuccess);
                Assert.AreEqual(CreateWalletResult.ExecutionMessage.Success, result.CurrentExecutionMessage);
            });
        }

        [Test]
        public async Task CreateWalletTestAmount()
        {
            var command = new CreateWalletCommand
            {
                UserId = "test_user_id",
                Currency = "BTC"
            };

            var result = await sut.Handle(command, CancellationToken.None);
            bool walletCreationSuccess = context.Wallets.Where(w => w.Currency == "BTC" && w.Amount == 500) != null;


            Assert.Multiple(() =>
            {
                Assert.IsTrue(result.IsSuccessful);
                Assert.IsTrue(walletCreationSuccess);
                Assert.AreEqual(500, result.Amount);
            });
        }

        [Test]
        public async Task CreateWalletInvalidCurrency()
        {
            var command = new CreateWalletCommand
            {
                UserId = "test_user_id",
                Currency = "RUB"
            };

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccessful);
                Assert.AreEqual(CreateWalletResult.ExecutionMessage.ErrorInvalidCurrency, result.CurrentExecutionMessage);
            });
        }

        [Test]
        public async Task CreateWalletExistingCurrency()
        {
            var command = new CreateWalletCommand
            {
                UserId = "test_user_id",
                Currency = "EC"
            };

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccessful);
                Assert.AreEqual(CreateWalletResult.ExecutionMessage.ErrorWalletAlreadyExists, result.CurrentExecutionMessage);
            });
        }
    }
}
