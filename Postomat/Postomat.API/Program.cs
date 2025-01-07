using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Postomat.API.Middlewares;
using Postomat.Application.Services;
using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.DataAccess.Database.Context;
using Postomat.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false,
            SignatureValidator = (token, _) =>
                new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler().ReadJsonWebToken(token)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
                return Task.CompletedTask;
            }
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Service for data initialization
builder.Services.AddTransient<IDataInitializationService, DataInitializationService>();

// Services for controllers
builder.Services.AddTransient<IAccessCheckService, AccessCheckService>();
builder.Services.AddTransient<IAuthorizationService, AuthorizationService>();
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IDeliveryService, DeliveryService>();
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
builder.Services.AddCors(options => options.AddPolicy(
    "PostomatApiPolicy", b => b
        .WithOrigins(builder.Configuration.GetSection("Frontend")["FrontendAddress"] ?? string.Empty)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

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

    x.AddRequestClient<MicroserviceCreateLogRequest>();
    x.AddRequestClient<MicroserviceDeleteLogRequest>();
    x.AddRequestClient<MicroserviceGetFilteredLogsRequest>();
    x.AddRequestClient<MicroserviceGetLogRequest>();
    x.AddRequestClient<MicroserviceLoginUserRequest>();
    x.AddRequestClient<MicroserviceUpdateLogRequest>();
    x.AddRequestClient<MicroserviceValidateTokenRequest>();
});

var app = builder.Build();

// Error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Authentication
app.UseAuthentication();

// Authorization
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();

// Data initialization
using (var scope = app.Services.CreateScope())
{
    try
    {
        var initializer = scope.ServiceProvider.GetRequiredService<IDataInitializationService>();
        await initializer.InitializeData(new CancellationToken());

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IDataInitializationService>>();
        logger.LogInformation("Start data initialized successfully");
    }
    catch (Exception e)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IDataInitializationService>>();
        logger.LogCritical(e, "An error occurred during data initialization");

        Environment.Exit(1);
    }
}

app.Run();