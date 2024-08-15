using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

/* Add database */
builder.Services.AddDbContext<SampleRabbit.DB.DataAccess.SampleDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Sample"));
});

var rabbitMqConfiguration = builder.Configuration
    .GetSection(nameof(SampleRabbit.Shared.Config.RabbitMQConfiguration))
    .Get<SampleRabbit.Shared.Config.RabbitMQConfiguration>()!;

/* Add MassTransit */
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
        cfg.Host(new Uri(rabbitMqConfiguration.Host), h =>
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
