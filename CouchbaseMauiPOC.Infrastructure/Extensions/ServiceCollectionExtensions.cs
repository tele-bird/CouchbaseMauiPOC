using CouchbaseMauiPOC.Infrastructure.Repositories;
using CouchbaseMauiPOC.Infrastructure.Services;
using CouchbaseMauiPOC.Infrastructure.Sources;
using Microsoft.Extensions.DependencyInjection;

namespace CouchbaseMauiPOC.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddSingleton<UniversityDataSource>();
        services.AddSingleton<UserProfileDataSource>();
		services.AddSingleton<IUserProfileRepository, UserProfileRepository>();
		services.AddSingleton<IUniversityRepository, UniversityRepository>();

        return services;
    }
}
