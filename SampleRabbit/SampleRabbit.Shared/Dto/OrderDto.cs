using FluentValidation;
using System.Text.Json.Serialization;

namespace SampleRabbit.Shared.Dto
{
    /// <summary>
    ///     Rest API data transfer object
    /// </summary>
    public class OrderDto
    {
        /// <summary>
        ///     Ordered products
        /// </summary>
        [JsonPropertyName(name: "orderedProducts")]
        public IReadOnlyList<ProductDto>? OrderedProducts { get; set; }


        public class Validator : AbstractValidator<OrderDto>
        {
            public Validator(IValidator<ProductDto> validator)
            {
                this.RuleFor(order => order.OrderedProducts)
                    .NotEmpty().WithMessage("No order to create");

                this.RuleFor(order => order.OrderedProducts)
                    .ForEach(item => item.SetValidator(validator));
            }
        }
    }
}
