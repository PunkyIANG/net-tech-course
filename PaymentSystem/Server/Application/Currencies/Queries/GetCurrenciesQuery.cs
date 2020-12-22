using MediatR;
using PaymentSystem.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Application.Currencies.Queries
{
    public class GetCurrenciesQuery : IRequest<CurrencyList> 
    {

    }

    public class GetCurrenciesQueryHandler : IRequestHandler<GetCurrenciesQuery, CurrencyList>
    {
        private readonly ICurrencyManager currencyManager;

        public GetCurrenciesQueryHandler(ICurrencyManager currencyManager)
        {
            this.currencyManager = currencyManager;
        }

        public async Task<CurrencyList> Handle(GetCurrenciesQuery query, CancellationToken cancellationToken)
        {
            return new CurrencyList
            {
                Currencies = currencyManager.GetCurrencies()
            };
        }
    } 
}
