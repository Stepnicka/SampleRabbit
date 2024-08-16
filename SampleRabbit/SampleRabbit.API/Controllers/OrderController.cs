using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

using SampleRabbit.Shared.Dto;
using SampleRabbit.Shared.Error;

namespace SampleRabbit.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            this._mediator = mediator;
        }

        /// <summary>
        ///     Create new order
        /// </summary>
        /// <response code="202"> Order was accepted </response>
        /// <response code="401"> Validation error </response>
        /// <response code="500"> Internal error </response>
        [HttpPost()]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(statusCode: StatusCodes.Status202Accepted, type: typeof(void))]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(SimpleError))]
        [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError, type: typeof(SimpleError))]
        public async Task<IActionResult> Create([FromBody] OrderDto? order)
        {
            var handlerResult = await _mediator.Send(new Handlers.Order.PublishOrderCommand { Order = order });

            return handlerResult.Match(
                    result => (IActionResult)Ok(),
                    error => error switch
                    {
                        ValidationError validationError => BadRequest(SimpleError.OnValidation(validationError)),
                        /* Default case */
                        _ => StatusCode(StatusCodes.Status500InternalServerError, SimpleError.Generic),
                    });
        }
    }
}
