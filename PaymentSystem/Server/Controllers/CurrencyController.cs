using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentSystem.Shared;
using PaymentSystem.Server;
using PaymentSystem.Server.Application.Currencies.Queries;
using MediatR;

namespace PaymentSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly IMediator mediator;

        public CurrencyController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<CurrencyList> GetCurrenciesAsync()
        {
            var getCurrenciesQuery = new GetCurrenciesQuery();

            var getCurrenciesQueryResult = await mediator.Send(getCurrenciesQuery);

            return getCurrenciesQueryResult;
        }
    }
}
