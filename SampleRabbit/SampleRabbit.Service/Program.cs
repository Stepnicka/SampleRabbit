using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using SampleRabbit.Service.Consumers;

var builder = WebApplication.CreateBuilder(args);

/* Configure logging */
builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(configure => configure.AddConsoleExporter());

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
    busConfig.AddConsumer<CreateOrderConsumer>().Endpoint(endpointConfiguration =>
    {
        endpointConfiguration.PrefetchCount = 100;
        endpointConfiguration.Temporary = false;
    });

    busConfig.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        cfg.UseEntityFrameworkOutbox<SampleRabbit.DB.DataAccess.SampleDbContext>(context);
    });

    busConfig.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(host: rabbitMqConfiguration.Host, virtualHost: rabbitMqConfiguration.VirtualHost, h =>
        {
            h.Username(rabbitMqConfiguration.UserName);
            h.Password(rabbitMqConfiguration.Password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.Run();
