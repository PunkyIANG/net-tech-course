using MediatR;
using PaymentSystem.Server.Data;
using PaymentSystem.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentSystem.Server.Application.Users.Queries
{
    public class GetUserValidation : IRequest<UserValidationResult>
    {
        public string Username { get; set; }
    }

    public class GetUserValidationHandler : IRequestHandler<GetUserValidation, UserValidationResult>
    {
        private readonly ApplicationDbContext context;

        public GetUserValidationHandler(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<UserValidationResult> Handle(GetUserValidation query, CancellationToken cancellationToken)
        {
            var user = context.Users.FirstOrDefault(x => x.UserName == query.Username);

            return new UserValidationResult
            {
                Exists = user != null
            };
        }
    }
}
