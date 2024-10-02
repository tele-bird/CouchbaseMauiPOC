using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CouchbaseMauiPOC.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    [ObservableProperty]
    string? username;
    [ObservableProperty]
    string? password;

    private Action? signInSuccessful;

    public LoginViewModel(Action signInSuccessful)
    {
        this.signInSuccessful = signInSuccessful;
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

            signInSuccessful?.Invoke();
            signInSuccessful = null;
        }
    }
}
