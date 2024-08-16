using FluentValidation;
using MassTransit;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OneOf;
using OpenTelemetry.Logs;
using SampleRabbit.Shared.Error;

var builder = WebApplication.CreateBuilder(args);

/* Configure logging */
builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(configure => configure.AddConsoleExporter());

/* Add validators */
builder.Services.AddValidatorsFromAssemblyContaining<SampleRabbit.Handlers.ReferenceClass>();
builder.Services.AddValidatorsFromAssemblyContaining<SampleRabbit.Shared.ReferenceClass>();

/* Add database */
builder.Services.AddDbContext<SampleRabbit.DB.DataAccess.SampleDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Sample"));
});

/* Add mediatr */
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssemblyContaining<SampleRabbit.Handlers.ReferenceClass>();
    configuration.AddBehavior<IPipelineBehavior<SampleRabbit.Handlers.Order.PublishOrderCommand,OneOf<SampleRabbit.Handlers.Order.PublishOrderResult,SampleRabbit.Shared.Error.IDomainError>>, SampleRabbit.Handlers.ValidationBehavior<SampleRabbit.Handlers.Order.PublishOrderCommand, SampleRabbit.Handlers.Order.PublishOrderResult>>();
});

/* Add mediatr exception handlers */
builder.Services.AddTransient<IRequestExceptionHandler<SampleRabbit.Handlers.Order.PublishOrderCommand, OneOf<SampleRabbit.Handlers.Order.PublishOrderResult, IDomainError> ,Exception>, SampleRabbit.Handlers.Order.PublishOrderExceptionHandler>();

/* Add MassTransit */
var rabbitMqConfiguration = builder.Configuration
    .GetSection(nameof(SampleRabbit.Shared.Config.RabbitMQConfiguration))
    .Get<SampleRabbit.Shared.Config.RabbitMQConfiguration>()!;

builder.Services.AddMassTransit(busConfig =>
{
    busConfig.AddEntityFrameworkOutbox<SampleRabbit.DB.DataAccess.SampleDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(30);
        o.UseSqlServer().UseBusOutbox();
    });

    busConfig.SetKebabCaseEndpointNameFormatter();

    busConfig.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(host: rabbitMqConfiguration.Host, virtualHost: rabbitMqConfiguration.VirtualHost, h =>
        {
            h.Username(rabbitMqConfiguration.UserName);
            h.Password(rabbitMqConfiguration.Password);
        });

        cfg.UseMessageRetry(r => r.Exponential(10, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(5)));

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
