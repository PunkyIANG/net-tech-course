using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Helpers
{
    public class BoolResult
    {
        public bool IsSuccessful { get; set; }

        public static BoolResult ReturnSuccess()
        {
            return new BoolResult { IsSuccessful = true };
        }

        public static BoolResult ReturnFailure()
        {
            return new BoolResult { IsSuccessful = false };
        }
    }

}
