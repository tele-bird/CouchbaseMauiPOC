using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.ViewModels;

public partial class LoginViewModel : BaseNavigationViewModel
{
    [ObservableProperty]
    string? username;
    [ObservableProperty]
    string? password;

    public LoginViewModel(INavigationService navigationService)
        : base(navigationService)
    {
        Username = "myusername";
        Password = "mypassword";
    }

    [RelayCommand]
    void SignIn()
    {
        if(!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
        {
            AppInstance.User = new Models.UserCredentials
            {
                Username = Username,
                Password = Password
            };

            navigationService.PushAsync<UserProfileViewModel>(true);
        }
    }
}
