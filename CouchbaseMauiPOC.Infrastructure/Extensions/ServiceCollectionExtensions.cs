using CouchbaseMauiPOC.Infrastructure.Models;
using CouchbaseMauiPOC.Infrastructure.Repositories;
using CouchbaseMauiPOC.Infrastructure.Services;
using CouchbaseMauiPOC.Infrastructure.Sources;
using Microsoft.Extensions.DependencyInjection;

namespace CouchbaseMauiPOC.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static string UserProfilesDbName = "userprofiles";
    public static string UniversitiesDbName = "universities";

    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddKeyedSingleton<CouchbaseDataSource>(
            UserProfilesDbName, 
            (serviceProvider, obj) =>
            {
                return new CouchbaseDataSource( new DataSourceConfiguration
                {
                    DatabaseConfiguration = new DatabaseConfiguration
                    {
                        DatabaseName = (string)obj!,
                        IndexName = null,
                        Indexes = null // no indexes
                    },
                    SyncConfiguration = new DataSyncConfiguration
                    {
                        Username = "myusername",
                        Password = "mypassword",
                        EndpointUrl = "wss://ofw9ujauhjnzsf1f.apps.cloud.couchbase.com:4984/userprofile-endpoint",
                        IsContinuous = true,
                        ReplicatorType = Couchbase.Lite.Sync.ReplicatorType.PushAndPull
                    }
                });
            });
            
        services.AddKeyedSingleton<CouchbaseDataSource>(
            UniversitiesDbName, 
            (serviceProvider, obj) =>
            {
                return new CouchbaseDataSource( new DataSourceConfiguration
                {
                    DatabaseConfiguration = new DatabaseConfiguration
                    {
                        DatabaseName = (string)obj!,
                        IndexName = "NameLocationIndex",
                        Indexes = new [] { "name", "location" }
                    },
                    SyncConfiguration = null // not synchronized
                },
                serviceProvider.GetRequiredService<IDatabaseSeedService>());
            });
		services.AddTransient<IUserProfileRepository, UserProfileRepository>(serviceProvider =>
        {
            return new UserProfileRepository(
                serviceProvider.GetKeyedService<CouchbaseDataSource>(UserProfilesDbName)!);
        });
		services.AddTransient<IUniversityRepository, UniversityRepository>(serviceProvider =>
        {
            return new UniversityRepository(
                serviceProvider.GetKeyedService<CouchbaseDataSource>(UniversitiesDbName)!);
        });

        return services;
    }
}
