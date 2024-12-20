using Postomat.DataAccess.Database.Context;
using MassTransit;
using Postomat.Application.Services;
using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Abstractions.Services;
using Postomat.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services for controllers
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IDeliveryService, DeliveryService>();
builder.Services.AddTransient<IAuthorizationService, AuthorizationService>();
builder.Services.AddTransient<ILogsService, LogsService>();
builder.Services.AddTransient<IOrderPlansService, OrderPlansService>();
builder.Services.AddTransient<IOrdersService, OrdersService>();
builder.Services.AddTransient<IPostomatsService, PostomatsService>();
builder.Services.AddTransient<IRolesService, RolesService>();
builder.Services.AddTransient<IUsersService, UsersService>();

// Services for repositories
builder.Services.AddTransient<ICellsRepository, CellsRepository>();
builder.Services.AddTransient<ILogsRepository, LogsRepository>();
builder.Services.AddTransient<IOrderPlansRepository, OrderPlansRepository>();
builder.Services.AddTransient<IOrdersRepository, OrdersRepository>();
builder.Services.AddTransient<IPostomatsRepository, PostomatsRepository>();
builder.Services.AddTransient<IRolesRepository, RolesRepository>();
builder.Services.AddTransient<IUsersRepository, UsersRepository>();

// Database context
builder.Services.AddDbContext<PostomatDbContext>();

// CORS policy
builder.Services.AddCors(options => options.AddPolicy
    (
        "PostomatApiPolicy", b => b
            .WithOrigins
            (
                "https://localhost:5173" // frontend address
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    )
);

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("admin_user");
            h.Password("admin");
        });

        cfg.ReceiveEndpoint("auth_service", e =>
        {
            e.ConfigureConsumers(context);
        });

        cfg.ReceiveEndpoint("logging_service", e =>
        {
            e.ConfigureConsumers(context);
        });
    });

    x.AddConsumer<AuthRequestConsumer>();
    x.AddConsumer<LoggingRequestConsumer>();
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();