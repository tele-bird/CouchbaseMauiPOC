using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class UserProfilePage : BaseContentPage<UserProfileViewModel>
{
	public UserProfilePage(UserProfileViewModel userProfileViewModel)
		: base(userProfileViewModel)
	{
		InitializeComponent();
	}
}