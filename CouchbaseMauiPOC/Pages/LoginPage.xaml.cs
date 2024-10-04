using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class LoginPage : BaseContentPage<LoginViewModel>
{
	public LoginPage(LoginViewModel loginViewModel)
		: base(loginViewModel)
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		userNameEntry.Focus();
    }
}