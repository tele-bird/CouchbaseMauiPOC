using CouchbaseMauiPOC.Repositories;
using CouchbaseMauiPOC.Services;
using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class UserProfilePage : ContentPage
{
	public UserProfilePage(IServiceProvider serviceProvider, Action logoutSuccessful)
	{
		InitializeComponent();

		var userProfileRepository = serviceProvider.GetRequiredService<IUserProfileRepository>();
		var alertService = serviceProvider.GetRequiredService<IAlertService>();
		var mediaService = serviceProvider.GetRequiredService<IMediaService>();

		BindingContext = new UserProfileViewModel(userProfileRepository, alertService, mediaService, logoutSuccessful);
	}
}