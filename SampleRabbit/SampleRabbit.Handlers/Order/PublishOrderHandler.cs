using FluentValidation;
using MassTransit;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using OneOf;
using SampleRabbit.DB.DataAccess;
using SampleRabbit.Shared.Dto;
using SampleRabbit.Shared.Error;

namespace SampleRabbit.Handlers.Order
{
    /// <summary>
    ///     Publish new order into messaging pipeline
    /// </summary>
    /// <remarks>
    ///     Handles <see cref="PublishOrderCommand"/>, returns <see cref="PublishOrderResult"/> or <see cref="IDomainError"/>
    /// </remarks>
    public class PublishOrderHandler : IRequestHandler<PublishOrderCommand, OneOf<PublishOrderResult, IDomainError>>
    {
        private readonly IPublishEndpoint publishEndpoint;
        private readonly SampleDbContext dbContext;

        public PublishOrderHandler(IPublishEndpoint publishEndpoint, SampleDbContext dbContext)
        {
            this.publishEndpoint = publishEndpoint;
            this.dbContext = dbContext;
        }

        public async Task<OneOf<PublishOrderResult, IDomainError>> Handle(PublishOrderCommand request, CancellationToken cancellationToken)
        {
            await publishEndpoint.Publish(new Shared.Messages.CreateOrderMessage { CorrelationId = Guid.NewGuid(), OrderDto = request.Order! }, cancellationToken);

            /* Masstransit adds messages to outBox table, does not save in case user wants to have a transaction, we have to save ourselfs. */
            await dbContext.SaveChangesAsync(); 

            return new PublishOrderResult();
        }
    }

    /// <remarks> Handles exceptions for <see cref="PublishOrderHandler"/> </remarks>
    public class PublishOrderExceptionHandler : IRequestExceptionHandler<PublishOrderCommand, OneOf<PublishOrderResult, IDomainError>, Exception>
    {
        private readonly ILogger logger;

        public PublishOrderExceptionHandler(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<PublishOrderHandler>();
        }

        public Task Handle(PublishOrderCommand request, Exception exception, RequestExceptionHandlerState<OneOf<PublishOrderResult, IDomainError>> state, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Command {request} failed.", request);

            state.SetHandled(new InternalError { Message = "Ups... something failed." });

            return Task.CompletedTask;
        }
    }

    /// <remarks> Handled by <see cref="PublishOrderHandler"/> </remarks>
    public record PublishOrderCommand : IRequest<OneOf<PublishOrderResult, IDomainError>>
    {
        /// <summary>
        ///     Order to be published
        /// </summary>
        public OrderDto? Order { get; set; }

        public class Validator : AbstractValidator<PublishOrderCommand>
        {
            public Validator(IValidator<OrderDto> orderValidator)
            {
                this.RuleFor(command => command.Order)
                    .NotNull().WithMessage("No order to create");

                this.RuleFor(command => command.Order)
                    .SetValidator(orderValidator!);
            }
        }

    }

    /// <remarks> Returned by <see cref="PublishOrderHandler"/> </remarks>
    public record PublishOrderResult
    {

    }
}
