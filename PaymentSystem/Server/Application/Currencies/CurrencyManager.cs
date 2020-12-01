using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Application.Currencies
{
    public class CurrencyManager : ICurrencyManager
    {
        private List<string> Currencies { get; }

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

        public List<string> GetCurrencies()
        {
            return Currencies;
        }
    }
}
