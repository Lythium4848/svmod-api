using Microsoft.EntityFrameworkCore;
using SVModAPI;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

builder.Services.AddDbContext<ApiDbContext>(options =>
    options
        .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
);

builder.Services.AddControllers();

builder.Services.AddScoped<ApiKeyAuthFilter>();

var apiKey = Environment.GetEnvironmentVariable("API_KEY");
builder.Services.AddSingleton(new ApiKeyStore(apiKey ?? throw new InvalidOperationException("API_KEY environment variable is not set.")));


var app = builder.Build();

//app.UseHttpsRedirection();

app.UseRouting();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApiDbContext>();
    context.Database.EnsureCreated(); 
}

app.Run();

public class ApiKeyStore(string apiKey)
{
    public string ApiKey { get; } = apiKey;
}