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
        await LoadUserProfileAsync();
    }

    private async Task LoadUserProfileAsync()
    {
        IsBusy = true;

        var userprofile = await userProfileRepository.GetLocalAsync(UserProfileDocId);
        if(userprofile == null)
        {
            userprofile = new UserProfile
            {
                Id = UserProfileDocId,
                Email = AppInstance.User!.Username
            };
        }

        Name = userprofile.Name;
        Email = userprofile.Email;
        Address = userprofile.Address;
        ImageData = userprofile.ImageData;
        University = userprofile.University;

        await userProfileRepository.StartReplicationForCurrentUser().ConfigureAwait(false);

        userProfileRepository.UserProfileResultsChanged += UpdateUserProfileResults;
        await userProfileRepository.GetAsync(UserProfileDocId);

        IsBusy = false;
    }

    private void UpdateUserProfileResults(QueryResultsChangedEventArgs<UserProfile> args)
    {
        try
        {
            if(args.Exception != null)
            {
                alertService.ShowMessage(args.Exception.GetType().Name, args.Exception.Message, "OK");
            }
            else if(args.DataEntities != null)
            {
                if(args.DataEntities.Count != 1)
                {
                    throw new ArgumentOutOfRangeException($"Unexpected scenario: ");
                }
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Name = args.DataEntities[0].Name;
                    Email = args.DataEntities[0].Email;
                    Address = args.DataEntities[0].Address;
                    ImageData = args.DataEntities[0].ImageData;
                    University = args.DataEntities[0].University;
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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                University = name;
            });
        };
    }
}
