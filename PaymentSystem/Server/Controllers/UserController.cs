using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentSystem.Server.Application.Users.Queries;
using PaymentSystem.Server.Data;
using PaymentSystem.Shared;

namespace PaymentSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMediator mediator;
        public UserController(ApplicationDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        [HttpGet]
        [Route("{username}/validate")]
        public async Task<UserValidationResult> ValidateUserAsync(string username)
        {
            var getUserValidation = new GetUserValidation
            {
                Username = username
            };

            var getUserValidationResult = await mediator.Send(getUserValidation);

            return getUserValidationResult;
        }
    }
}
