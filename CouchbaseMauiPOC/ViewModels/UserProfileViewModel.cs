using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CouchbaseMauiPOC.Models;
using CouchbaseMauiPOC.Repositories;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.ViewModels;

public partial class UserProfileViewModel : BaseViewModel
{
    private readonly IUserProfileRepository userProfileRepository;
    private readonly IAlertService alertService;
    private readonly IMediaService mediaService;
    private Action? logoutSuccessful;

    [ObservableProperty]
    string? name;
    [ObservableProperty]
    string? email;
    [ObservableProperty]
    string? address;
    [ObservableProperty]
    byte[]? imageData;

    string UserProfileDocId => $"user::{AppInstance.User!.Username}";

    public UserProfileViewModel(
        IUserProfileRepository userProfileRepository, 
        IAlertService alertService, 
        IMediaService mediaService, 
        Action logoutSuccessful)
    {
        this.userProfileRepository = userProfileRepository;
        this.alertService = alertService;
        this.mediaService = mediaService;
        this.logoutSuccessful = logoutSuccessful;

        LoadUserProfile();
    }

    private async void LoadUserProfile()
    {
        IsBusy = true;

        var userProfile = await Task.Run(() =>
        {
            var up = userProfileRepository.Get(UserProfileDocId);
            if(up == null)
            {
                up = new UserProfile
                {
                    Id = UserProfileDocId,
                    Email = AppInstance.User!.Username
                };
            }

            return up;
        });

        if(userProfile != null)
        {
            Name = userProfile.Name;
            Email = userProfile.Email;
            Address = userProfile.Address;
            ImageData = userProfile.ImageData;
        }

        IsBusy = false;
    }

    [RelayCommand]
    Task Save()
    {
        var userProfile = new UserProfile
        {
            Id = UserProfileDocId,
            Name = Name,
            Email = Email,
            Address = Address,
            ImageData = ImageData
        };

        bool? success = userProfileRepository.Save(userProfile);

        if(success.HasValue && success.Value)
        {
            return alertService.ShowMessage(string.Empty, "Successfully updated profile", "OK");
        }

        return alertService.ShowMessage(string.Empty, "Error updating profile", "OK");
    }

    [RelayCommand]
    async Task SelectImage()
    {
        var imageData = await mediaService.PickPhotoAsync();

        if(imageData != null)
        {
            ImageData = imageData;
        }
    }

    [RelayCommand]
    void Logout()
    {
        userProfileRepository.Dispose();
        AppInstance.User = null;
        logoutSuccessful?.Invoke();
        logoutSuccessful = null;
    }
}
