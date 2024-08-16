using FluentValidation.Results;

namespace SampleRabbit.Shared.Error
{
    /// <summary>
    ///     Error on validation
    /// </summary>
    public class ValidationError : IDomainError
    {
        /// <summary>
        ///     Failures returned by validation
        /// </summary>
        public IReadOnlyList<ValidationFailure> ValidationFailures { get; set; } = null!;
    }
}
