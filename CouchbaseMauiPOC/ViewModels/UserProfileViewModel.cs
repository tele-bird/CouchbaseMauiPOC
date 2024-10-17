using System.Diagnostics;
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
    string? universityName;

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

    public override async Task OnFirstAppearingAsync(bool refresh)
    {
        await LoadUserProfileAsync();
        // await userProfileRepository.StartReplicationForCurrentUser().ConfigureAwait(false);
    }

    private async Task LoadUserProfileAsync()
    {
        IsBusy = true;
        try
        {
            userProfileRepository.UserProfileResultChanged += UpdateUserProfileResult;
            await userProfileRepository.GetAsync(UserProfileDocId);
        }
        catch(Exception exc)
        {
            await alertService.ShowMessage(exc.GetType().FullName!, exc.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateUserProfileResult(QueryResultChangedEventArgs<UserProfile> args)
    {
        try
        {
            if(args.Exception != null)
            {
                alertService.ShowMessage(args.Exception.GetType().Name, args.Exception.Message, "OK");
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Name = args.DataEntity?.Name;
                    Email = args.DataEntity?.Email;
                    Address = args.DataEntity?.Address;
                    ImageData = args.DataEntity?.ImageData;
                    UniversityName = args.DataEntity?.University;
                });
            }
        }
        catch(Exception exc)
        {
            alertService.ShowMessage(exc.GetType().Name, exc.Message, "OK");
        }
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
            University = UniversityName
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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UniversityName = name;
            });
        };
    }
}
