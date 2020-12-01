using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Helpers
{
    public class BoolResult
    {
        public bool IsSuccessful { get; set; }
        public string SuccessMessage { get; set; }
        public string FailureReason { get; set; }

        public static BoolResult ReturnSuccess()
        {
            return new BoolResult { IsSuccessful = true };
        }

        public static BoolResult ReturnSuccess(string successMessage)
        {
            return new BoolResult { 
                IsSuccessful = true,
                SuccessMessage = successMessage
            };
        }

        public static BoolResult ReturnFailure()
        {
            return new BoolResult { IsSuccessful = false };
        }

        public static BoolResult ReturnFailure(string failureReason) {
            return new BoolResult
            {
                IsSuccessful = false,
                FailureReason = failureReason
            };
        }
    }

}
