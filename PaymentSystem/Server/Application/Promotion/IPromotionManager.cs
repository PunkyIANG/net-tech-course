using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Application.Promotion
{
    public interface IPromotionManager
    {
        decimal GetDefaultAmount(string currency);
    }
}
