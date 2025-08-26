using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MiniSocial.Core.Interfaces;
using MiniSocial.Core.Services;
using MiniSocial.Core.Entities;
using MiniSocial.Infrastructure.Configuration;
using MiniSocial.Infrastructure.Repositories;

namespace MiniSocial.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure MongoDB serialization conventions
        ConfigureMongoDbSerialization();

        // Configure MongoDB
        var mongoDbSettings = configuration.GetSection("MongoDb").Get<MongoDbSettings>();
        if (mongoDbSettings == null)
            throw new InvalidOperationException("MongoDb configuration section is missing");

        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));

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

        // Register services
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }

    private static void ConfigureMongoDbSerialization()
    {
        // Only register conventions and class maps if not already registered
        if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
        {
            // Configure camelCase naming convention for MongoDB field names
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("camelCase", conventionPack, t => true);
        
            BsonClassMap.RegisterClassMap<User>(cm =>
            {
                cm.AutoMap();
                // Map the Id property to MongoDB's _id field
                cm.MapIdMember(c => c.Id);
                cm.MapMember(c => c.Username).SetElementName("username");
                cm.MapMember(c => c.Email).SetElementName("email");
                cm.MapMember(c => c.PasswordHash).SetElementName("passwordHash");
                cm.MapMember(c => c.Bio).SetElementName("bio").SetIgnoreIfNull(true);
                cm.MapMember(c => c.ProfilePictureUrl).SetElementName("profilePictureUrl").SetIgnoreIfNull(true);
                cm.MapMember(c => c.CreatedAt).SetElementName("createdAt");
                cm.MapMember(c => c.FollowersCount).SetElementName("followersCount");
                cm.MapMember(c => c.FollowingCount).SetElementName("followingCount");
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}
