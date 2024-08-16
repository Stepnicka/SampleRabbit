using MassTransit;
using SampleRabbit.Shared.Dto;

namespace SampleRabbit.Shared.Messages
{
    public class CreateOrderMessage : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }

        public OrderDto OrderDto { get; set; } = null!;
    }
}
