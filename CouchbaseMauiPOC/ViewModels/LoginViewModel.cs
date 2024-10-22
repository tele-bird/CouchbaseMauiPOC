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

    public LoginViewModel(
        INavigationService navigationService,
        IAlertService alertService)
        : base(navigationService, alertService)
    {
        Username = "myusername";
        Password = "mypassword";
    }

    [RelayCommand]
    async Task SignInAsync()
    {
        try
        {
            if(!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                AppInstance.User = new Models.UserCredentials
                {
                    Username = Username,
                    Password = Password
                };

                await navigationService.PushAsync<UserProfileViewModel>(true, (vm) =>
                {
                    vm.UserProfile.Email = Username;
                });
            }
        }
        catch(Exception exc)
        {
            await HandleExceptionAsync(exc);
        }
    }
}
