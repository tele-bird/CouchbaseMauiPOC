using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class LoginPage : ContentPage
{
	public LoginPage(Action signInSuccessful)
	{
		InitializeComponent();
		BindingContext = new LoginViewModel(signInSuccessful);
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		userNameEntry.Focus();
    }
}