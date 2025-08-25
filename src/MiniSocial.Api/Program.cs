using MiniSocial.Infrastructure.Extensions;
using MiniSocial.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Configure multiple secret sources (in order of priority)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables() // Environment variables (for containers/cloud)
    .AddUserSecrets<Program>(); // User Secrets (for local development)

// Add cloud-specific secret sources based on environment
if (builder.Environment.IsProduction())
{
    // TODO: Add cloud secret providers when deploying
    // Examples:
    // - Azure Key Vault: builder.Configuration.AddAzureKeyVault(...)
    // - AWS Secrets Manager: builder.Configuration.AddSecretsManager(...)
    // - HashiCorp Vault: builder.Configuration.AddVault(...)
}

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Register MongoDB and repositories
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Map user registration endpoint
app.MapUserEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
