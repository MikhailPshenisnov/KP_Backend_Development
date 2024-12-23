using Postomat.DataAccess.Database.Context;
using MassTransit;
using Postomat.Application.Services;
using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Abstractions.Services;
using Postomat.DataAccess.Repositories;
using Postomat.LoggingMicroservice.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Services for consumers
builder.Services.AddTransient<ILogsService, LogsService>();

// Services for repositories
builder.Services.AddTransient<ILogsRepository, LogsRepository>();

// Database context
builder.Services.AddDbContext<PostomatDbContext>();

// Message broker
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });

    x.AddConsumer<CreateLogConsumer>();
    x.AddConsumer<GetLogConsumer>();
    x.AddConsumer<GetFilteredLogsConsumer>();
    x.AddConsumer<UpdateLogConsumer>();
    x.AddConsumer<DeleteLogConsumer>();
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();