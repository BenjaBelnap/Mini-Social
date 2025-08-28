using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DotNetEnv;

namespace MiniSocial.Tests.Integration;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Load environment variables from database/.env file
        var envFilePath = Path.Combine(
            Directory.GetCurrentDirectory(), 
            "..", "..", "..", "..", "..", 
            "database", ".env");
            
        if (File.Exists(envFilePath))
        {
            Env.Load(envFilePath);
        }
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Set environment to Test
            context.HostingEnvironment.EnvironmentName = "Test";
            
            // Clear existing configuration and add test-specific settings
            config.Sources.Clear();
            config.AddJsonFile("appsettings.json", optional: false)
                  .AddJsonFile("appsettings.Test.json", optional: true)
                  .AddEnvironmentVariables()
                  .AddInMemoryCollection(new[]
                  {
                      new KeyValuePair<string, string?>("MongoDb:ConnectionString", 
                          Environment.GetEnvironmentVariable("MONGO_TEST_CONNECTION_STRING")
                          ?? BuildConnectionString()),
                      new KeyValuePair<string, string?>("MongoDb:DatabaseName", 
                          Environment.GetEnvironmentVariable("MONGO_TEST_DATABASE") ?? "minisocial_test")
                  });
        });

        builder.ConfigureServices(services =>
        {
            // Configure logging to be more verbose in tests
            services.Configure<LoggerFilterOptions>(options =>
            {
                options.MinLevel = LogLevel.Debug;
            });
        });

        builder.UseEnvironment("Test");
    }
    
    private static string BuildConnectionString()
    {
        var username = Environment.GetEnvironmentVariable("MONGO_TEST_USERNAME") ?? "minisocial_test_user";
        var password = Environment.GetEnvironmentVariable("MONGO_TEST_PASSWORD") ?? "minisocial_test_password";
        var host = Environment.GetEnvironmentVariable("MONGO_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("MONGO_PORT") ?? "27017";
        var database = Environment.GetEnvironmentVariable("MONGO_TEST_DATABASE") ?? "minisocial_test";
        
        return $"mongodb://{username}:{password}@{host}:{port}/{database}?authSource={database}";
    }
}
