using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MiniSocial.Core.Interfaces;
using MiniSocial.Infrastructure.Configuration;
using MiniSocial.Infrastructure.Repositories;

namespace MiniSocial.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure MongoDB
        var mongoDbSettings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
        if (mongoDbSettings == null)
            throw new InvalidOperationException("MongoDbSettings configuration section is missing");

        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

        // Register MongoDB client and database
        services.AddSingleton<IMongoClient>(provider =>
            new MongoClient(mongoDbSettings.ConnectionString));

        services.AddScoped<IMongoDatabase>(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoDbSettings.DatabaseName);
        });

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();

        return services;
    }
}
