using CouchbaseMauiPOC.Pages;
using CouchbaseMauiPOC.Services;
using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNavigation(this IServiceCollection services)
    {
    	services.AddSingleton<INavigationService, NavigationService>((sp) =>
        { 
            var navigationService = new NavigationService(sp);

            navigationService.Register<LoginViewModel, LoginPage>();
            navigationService.Register<UniversitiesViewModel, UniversitiesPage>();
            navigationService.Register<UserProfileViewModel, UserProfilePage>();
            navigationService.Register<UniversityViewModel, UniversityPage>();

            return navigationService;
        });

        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginPage>();
        services.AddTransient<UniversitiesViewModel>();
        services.AddTransient<UniversitiesPage>();
        services.AddTransient<UserProfileViewModel>();
        services.AddTransient<UserProfilePage>();
        services.AddTransient<UniversityViewModel>();
        services.AddTransient<UniversityPage>();
	
        return services;
    }
}
