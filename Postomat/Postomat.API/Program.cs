using Postomat.DataAccess.Database.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services for controllers
// builder.Services.AddTransient<IArticlesService, ArticlesService>();

// Services for repositories
// builder.Services.AddTransient<IArticlesRepository, ArticlesRepository>();

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

// Database context
builder.Services.AddDbContext<PostomatDbContext>();

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