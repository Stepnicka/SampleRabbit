namespace SampleRabbit.Shared.Error
{
    public record InternalError : IDomainError
    {
        public required string Message { get; init; }
    }
}
