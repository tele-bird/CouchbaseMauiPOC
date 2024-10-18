using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CouchbaseMauiPOC.Infrastructure.Events;
using CouchbaseMauiPOC.Infrastructure.Models;
using CouchbaseMauiPOC.Infrastructure.Repositories;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.ViewModels;

public partial class UserProfileViewModel : BaseNavigationViewModel
{
    private readonly IUserProfileRepository userProfileRepository;
    private readonly IUniversityRepository universityRepository;
    private readonly IAlertService alertService;
    private readonly IMediaService mediaService;

    [ObservableProperty]
    UserProfile userProfile;
    [ObservableProperty]
    University? university;

    private Guid guid;

    public UserProfileViewModel(
        INavigationService navigationService,
        IUserProfileRepository userProfileRepository, 
        IUniversityRepository universityRepository,
        IAlertService alertService, 
        IMediaService mediaService)
        : base(navigationService)
    {
        this.guid = Guid.NewGuid();
        this.userProfileRepository = userProfileRepository;
        this.universityRepository = universityRepository;
        this.alertService = alertService;
        this.mediaService = mediaService;
        this.UserProfile = new UserProfile
        {
            Email = AppInstance.User!.Username
        };
        userProfileRepository.UserProfileResultChanged += UpdateUserProfileResult;
        universityRepository.UniversityResultChanged += UpdateUniversityResult;
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
            ArgumentNullException.ThrowIfNull(UserProfile.Email);
            await userProfileRepository.GetAsync(UserProfile.Email);
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

    private async void UpdateUserProfileResult(QueryResultChangedEventArgs<UserProfile> args)
    {
        Trace.WriteLine($"{guid}: {nameof(UserProfileViewModel)}.{nameof(UpdateUserProfileResult)} >> Error:{args.Exception} DataEntity:{args.DataEntity?.Email}");
        try
        {
            if(args.Exception != null)
            {
                await alertService.ShowMessage(args.Exception.GetType().Name, args.Exception.Message, "OK");
            }
            else if(args.DataEntity != null)
            {
                UserProfile = args.DataEntity;
                if(UserProfile.UniversityId == null)
                {
                    University = null;
                }
                else
                {
                    await universityRepository.GetAsync(UserProfile.UniversityId);
                }
            }
        }
        catch(Exception exc)
        {
            await alertService.ShowMessage(exc.GetType().Name, exc.Message, "OK");
        }
    }

    private async void UpdateUniversityResult(QueryResultChangedEventArgs<University> args)
    {
        Trace.WriteLine($"{guid}: {nameof(UserProfileViewModel)}.{nameof(UpdateUniversityResult)} >> Error:{args.Exception} DataEntity:{args.DataEntity?.Name}");
        try
        {
            if(args.Exception != null)
            {
                await alertService.ShowMessage(args.Exception.GetType().Name, args.Exception.Message, "OK");
            }
            else if(args.DataEntity != null)
            {
                University = args.DataEntity;
            }
        }
        catch(Exception exc)
        {
            await alertService.ShowMessage(exc.GetType().Name, exc.Message, "OK");
        }
    }

    [RelayCommand]
    async Task Save()
    {
        if(string.IsNullOrEmpty(UserProfile.Name) || string.IsNullOrEmpty(UserProfile.Email) || string.IsNullOrEmpty(UserProfile.Address))
        {
            await alertService.ShowMessage("Missing", "Name, Email and Address are all required.", "OK");
        }
        else
        {
            if(University != null)
            {
                UserProfile.UniversityId = University.Id;
            }
            bool success = await userProfileRepository.SaveAsync(UserProfile).ConfigureAwait(false);

            if(success)
            {
                await alertService.ShowMessage(string.Empty, $"Successfully updated profile to {userProfileRepository.Path}", "OK");
            }
            else
            {
                await alertService.ShowMessage(string.Empty, $"Error updating profile to {userProfileRepository.Path}", "OK");
            }
        }
    }

    [RelayCommand]
    async Task SelectImage()
    {
        var imageData = await mediaService.PickPhotoAsync();

        if(imageData != null)
        {
            UserProfile!.ImageData = imageData;
        }
    }

    [RelayCommand]
    void Logout()
    {
        userProfileRepository.Dispose();
        AppInstance.User = null;
        navigationService.PopAsync(false);
        userProfileRepository.UserProfileResultChanged -= UpdateUserProfileResult;
        universityRepository.UniversityResultChanged -= UpdateUniversityResult;
    }

    [RelayCommand]
    Task SelectUniversity()
    {
        return navigationService.PushAsync<UniversitiesViewModel>(true, InitializeUniversitiesViewModel);
    }

    private void InitializeUniversitiesViewModel(UniversitiesViewModel universitiesViewModel)
    {
        universitiesViewModel.UniversitySelected = (university) =>
        {
            University = university;
        };
    }
}
