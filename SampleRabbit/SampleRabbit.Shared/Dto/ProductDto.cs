using FluentValidation;

namespace SampleRabbit.Shared.Dto
{
    /// <summary>
    ///     Rest api data transfer object
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        ///     Product unique identifier
        /// </summary>
        public int Id { get; set; }

        public class Validator : AbstractValidator<ProductDto>
        {
            public Validator()
            {
                this.RuleFor(product => product.Id)
                    .GreaterThan(0).WithMessage("Product Id cannot be less or equeal to 0");
            }
        }
    }
}
