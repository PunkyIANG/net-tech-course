using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Application.Currencies
{
    public interface ICurrencyManager
    {
        List<string> GetCurrencies();
    }
}
