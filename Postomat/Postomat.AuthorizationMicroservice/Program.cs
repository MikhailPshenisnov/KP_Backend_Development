using Postomat.DataAccess.Database.Context;
using MassTransit;
using Postomat.Application.Services;
using Postomat.AuthorizationMicroservice.Consumers;
using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Abstractions.Services;
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
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
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