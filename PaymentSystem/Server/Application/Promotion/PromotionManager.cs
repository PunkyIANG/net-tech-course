using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Application.Promotion
{
    public class PromotionManager : IPromotionManager
    {
        public decimal GetDefaultAmount(string currency)
        {
            if (currency == "BTC")
            {
                return 1000;
            }

            return 0;
        }
    }
}
