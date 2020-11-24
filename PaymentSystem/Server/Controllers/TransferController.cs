using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PaymentSystem.Server.Application.Transfers.Commands;
using PaymentSystem.Server.Application.Transfers.Queries;
using PaymentSystem.Server.Data;
using PaymentSystem.Server.Helpers;
using PaymentSystem.Server.Models;
using PaymentSystem.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMediator mediator;

        public TransferController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMediator mediator)
        {
            this.context = context;
            this.userManager = userManager;
            this.mediator = mediator;
        }


        [HttpGet]
        [Route("transfers/{itemsPerPage}/{pageNumber}")]
        public async Task<TransactionsHistoryData> GetTransactionsAsync(int itemsPerPage, int pageNumber, [FromQuery] Direction direction)
        {
            var getTransactionsQuery = new GetTransactionsQuery
            {
                UserId = userManager.GetUserId(User),
                ItemsPerPage = itemsPerPage,
                PageNumber = pageNumber,
                Direction = direction
            };

            var getTransactionsQueryResult = await mediator.Send(getTransactionsQuery);

            return getTransactionsQueryResult;
        }


        [HttpPost]
        [Route("transfer")]
        public async Task<IActionResult> MakeTransferAsync([FromBody] TransferDto data)
        {
            var makeTransferCommand = new MakeTransferCommand
            {
                UserId = userManager.GetUserId(User),
                Data = data
            };

            var makeTransferResult = await mediator.Send(makeTransferCommand);

            if (!makeTransferResult.IsSuccessful)
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}
