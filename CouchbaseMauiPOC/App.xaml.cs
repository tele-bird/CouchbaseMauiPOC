using CouchbaseMauiPOC.Pages;

namespace CouchbaseMauiPOC;

public partial class App : Application
{
    private readonly IServiceProvider serviceProvider;

    public App(IServiceProvider serviceProvider)
	{
		InitializeComponent();

		MainPage = new LoginPage(OnSignInSuccessful);
        this.serviceProvider = serviceProvider;
    }

	void OnSignInSuccessful()
	{
		var navPage = new NavigationPage(new UserProfilePage(serviceProvider, OnLogoutSuccessful));
		MainPage = navPage;
	}

	void OnLogoutSuccessful() => MainPage = new LoginPage(OnSignInSuccessful);
}
