using FluentValidation;
using MediatR;
using OneOf;
using SampleRabbit.Shared.Error;

namespace SampleRabbit.Handlers
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, OneOf<TResponse, IDomainError>>
        where TRequest : IRequest<OneOf<TResponse, IDomainError>>
    {
        private readonly IValidator<TRequest> _validator;

        public ValidationBehavior(IValidator<TRequest> validator)
        {
            this._validator = validator;
        }

        public async Task<OneOf<TResponse, IDomainError>> Handle(TRequest request, RequestHandlerDelegate<OneOf<TResponse, IDomainError>> next, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, CancellationToken.None);

            if (validationResult.IsValid == false)
            {
                return new ValidationError() { ValidationFailures = validationResult.Errors };
            }

            return await next();
        }
    }
}
