using MassTransit;
using Postomat.Application.Services;
using Postomat.AuthorizationMicroservice.Consumers;
using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Abstractions.Services;
using Postomat.DataAccess.Database.Context;
using Postomat.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Services for consumers
builder.Services.AddTransient<IAuthorizationService, AuthorizationService>();
builder.Services.AddTransient<IUsersService, UsersService>();

// Services for repositories
builder.Services.AddTransient<IUsersRepository, UsersRepository>();

// Database context
builder.Services.AddDbContext<PostomatDbContext>();

// Message broker
builder.Services.AddMassTransit(x =>
{
    var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqConfig["Host"] ?? string.Empty, h =>
        {
            h.Username(rabbitMqConfig["Username"] ?? string.Empty);
            h.Password(rabbitMqConfig["Password"] ?? string.Empty);
        });

        cfg.ConfigureEndpoints(context);
    });

    x.AddConsumer<LoginUserConsumer>();
    x.AddConsumer<ValidateTokenConsumer>();
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();