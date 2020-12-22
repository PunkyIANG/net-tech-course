using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Application.Currencies
{
    public class CurrencyManager : ICurrencyManager
    {
        private IEnumerable<string> Currencies { get; }

        public CurrencyManager()
        {
            Currencies = new List<string>
            {
                "USD",
                "EUR",
                "GBP",
                "MDL",
                "BTC",
                "EC"
            };
        }

        public IEnumerable<string> GetCurrencies()
        {
            return Currencies;
        }
    }
}
