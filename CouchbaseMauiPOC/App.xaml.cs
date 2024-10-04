using CouchbaseMauiPOC.Pages;
using CouchbaseMauiPOC.Services;
using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
	{
		InitializeComponent();

		var loginPage = serviceProvider.GetRequiredService<LoginPage>();
		MainPage = new NavigationPage(loginPage);
    }
}
