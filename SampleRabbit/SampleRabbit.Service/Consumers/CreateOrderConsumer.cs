using MassTransit;
using SampleRabbit.DB.DataAccess;
using SampleRabbit.DB.Models;
using SampleRabbit.Shared.Messages;

namespace SampleRabbit.Service.Consumers
{
    internal class CreateOrderConsumer : IConsumer<CreateOrderMessage>
    {
        private readonly SampleDbContext dbContext;

        public CreateOrderConsumer(SampleDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<CreateOrderMessage> context)
        {
            try
            {
                var prods = new Dictionary<int, Product>();
                var order = new Order { Products = new List<Product>() };

                foreach(var item in context.Message.OrderDto.OrderedProducts!)
                {
                    if (!prods.TryGetValue(item.Id, out var product))
                    {
                        product = await dbContext.Products.FindAsync(item.Id);

                        if(product is not null)
                            prods.Add(product.Id, product);
                    }

                    if(product is not null)
                        order.Products.Add(product);
                }

                dbContext.Orders.Add(order);

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
