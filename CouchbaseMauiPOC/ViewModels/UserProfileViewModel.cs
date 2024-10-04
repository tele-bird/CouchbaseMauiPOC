using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CouchbaseMauiPOC.Models;
using CouchbaseMauiPOC.Repositories;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.ViewModels;

public partial class UserProfileViewModel : BaseNavigationViewModel
{
    private readonly IUserProfileRepository userProfileRepository;
    private readonly IAlertService alertService;
    private readonly IMediaService mediaService;

    [ObservableProperty]
    string? name;
    [ObservableProperty]
    string? email;
    [ObservableProperty]
    string? address;
    [ObservableProperty]
    byte[]? imageData;
    [ObservableProperty]
    string? university;

    string UserProfileDocId => AppInstance.User != null ? $"user::{AppInstance.User!.Username}" : "user::";

    public UserProfileViewModel(
        INavigationService navigationService,
        IUserProfileRepository userProfileRepository, 
        IAlertService alertService, 
        IMediaService mediaService)
        : base(navigationService)
    {
        this.userProfileRepository = userProfileRepository;
        this.alertService = alertService;
        this.mediaService = mediaService;
    }

    public override async Task LoadAsync(bool refresh)
    {
        await LoadUserProfile();
    }

    private async Task LoadUserProfile()
    {
        IsBusy = true;

        if(string.IsNullOrEmpty(Email))
        {
            var userProfile = await userProfileRepository.GetAsync(UserProfileDocId);

            if(userProfile == null)
            {
                userProfile = new UserProfile
                {
                    Id = UserProfileDocId,
                    Email = AppInstance.User!.Username
                };
            }

            Name = userProfile.Name;
            Email = userProfile.Email;
            Address = userProfile.Address;
            ImageData = userProfile.ImageData;
            University = userProfile.University;
        }

        IsBusy = false;
    }

    [RelayCommand]
    async Task Save()
    {
        var userProfile = new UserProfile
        {
            Id = UserProfileDocId,
            Name = Name,
            Email = Email,
            Address = Address,
            ImageData = ImageData,
            University = University
        };

        bool success = await userProfileRepository.SaveAsync(userProfile).ConfigureAwait(false);

        if(success)
        {
            await alertService.ShowMessage(string.Empty, $"Successfully updated profile to {userProfileRepository.Path}", "OK");
        }
        else
        {
            await alertService.ShowMessage(string.Empty, $"Error updating profile to {userProfileRepository.Path}", "OK");
        }
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
        navigationService.PopAsync(false);
    }

    [RelayCommand]
    Task SelectUniversity()
    {
        return navigationService.PushAsync<UniversitiesViewModel>(true, InitializeUniversitiesViewModel);
    }

    private void InitializeUniversitiesViewModel(UniversitiesViewModel universitiesViewModel)
    {
        universitiesViewModel.UniversitySelected = (name) =>
        {
            University = name;
        };
    }
}
