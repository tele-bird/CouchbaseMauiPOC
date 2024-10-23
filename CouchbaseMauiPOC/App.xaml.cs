using CommunityToolkit.Maui;
using CouchbaseMauiPOC.Infrastructure.Sources;
using CouchbaseMauiPOC.Pages;
using CouchbaseMauiPOC.Services;
using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC;

public partial class App : Application
{
    private readonly CouchbaseDataSource? universitiesDataSource;
    private readonly CouchbaseDataSource? userProfilesDataSource;

    public App(IServiceProvider serviceProvider)
	{
		InitializeComponent();

		var loginPage = serviceProvider.GetRequiredService<LoginPage>();
		universitiesDataSource = serviceProvider.GetKeyedService<CouchbaseDataSource>(
			Infrastructure.Extensions.ServiceCollectionExtensions.UniversitiesDbName);
		userProfilesDataSource = serviceProvider.GetKeyedService<CouchbaseDataSource>(
			Infrastructure.Extensions.ServiceCollectionExtensions.UserProfilesDbName);
		MainPage = new NavigationPage(loginPage);
    }

    protected override void OnSleep()
    {
        base.OnSleep();
		universitiesDataSource?.Pause(CancellationToken.None);
		userProfilesDataSource?.Pause(CancellationToken.None);
    }

    protected override void OnResume()
    {
        base.OnResume();
		universitiesDataSource?.Resume(CancellationToken.None);
		userProfilesDataSource?.Resume(CancellationToken.None);
    }
}
