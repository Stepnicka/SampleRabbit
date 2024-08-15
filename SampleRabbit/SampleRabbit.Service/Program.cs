using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/* Add database */
builder.Services.AddDbContext<SampleRabbit.DB.DataAccess.SampleDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Sample"));
});


/* Add masstransit */
var rabbitMqConfiguration = builder.Configuration
    .GetSection(nameof(SampleRabbit.Shared.Config.RabbitMQConfiguration))
    .Get<SampleRabbit.Shared.Config.RabbitMQConfiguration>()!;

builder.Services.AddMassTransit(busConfig =>
{
    busConfig.AddEntityFrameworkOutbox<SampleRabbit.DB.DataAccess.SampleDbContext>(o =>
    {
        o.UseSqlServer();
    });

    busConfig.SetKebabCaseEndpointNameFormatter();

    /* Add consumers */
    //busConfig.AddConsumer<Obect>();

    busConfig.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        cfg.UseEntityFrameworkOutbox<SampleRabbit.DB.DataAccess.SampleDbContext>(context);
    });

    busConfig.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(rabbitMqConfiguration.Host), h =>
        {
            h.Username(rabbitMqConfiguration.UserName);
            h.Password(rabbitMqConfiguration.Password);
        });

        //cfg.ReceiveEndpoint("name", endpoint =>
        //{
        //    endpoint.Durable = true;
        //    endpoint.ConsumerPriority = 10;
        //    endpoint.PrefetchCount = 100;
        //    endpoint.ConfigureConsumer<Object>(context);
        //});
    });
});



var app = builder.Build();


app.Run();
