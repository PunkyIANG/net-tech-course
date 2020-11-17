using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentSystem.Server
{
    public static class CurrencyManager
    {
        public static List<string> Currencies { get; }

        static CurrencyManager()
        {
            Currencies = new List<string>
            {
                "USD",
                "EUR",
                "GBP",
                "MDL",
                "EC"
            };
        }
    }
}
