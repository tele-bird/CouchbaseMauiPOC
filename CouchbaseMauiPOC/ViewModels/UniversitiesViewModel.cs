using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CouchbaseMauiPOC.Infrastructure.Events;
using CouchbaseMauiPOC.Infrastructure.Extensions;
using CouchbaseMauiPOC.Infrastructure.Models;
using CouchbaseMauiPOC.Infrastructure.Repositories;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.ViewModels;

public partial class UniversitiesViewModel : BaseNavigationViewModel
{
    private readonly IUniversityRepository universityRepository;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSearchEnabled))]
    string? name;

    [ObservableProperty]
    string? country;

    public bool IsSearchEnabled => !string.IsNullOrEmpty(Name);

    [ObservableProperty]
    List<University>? universities;

    public Action<University>? UniversitySelected {get; set;}

    public UniversitiesViewModel(INavigationService navigationService, IUniversityRepository universityRepository, IAlertService alertService)
     : base(navigationService, alertService)
     {
        this.universityRepository = universityRepository;
    }

    private async Task RefreshUniversitiesAsync()
    {
        IsBusy = true;
        try
        {
            universityRepository.UniversityResultsChanged += UpdateUniversityResultsAsync;
            await universityRepository.StartsWith(Name, Country);
        }
        catch(Exception exc)
        {
            await HandleExceptionAsync(exc);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void UpdateUniversityResultsAsync(QueryResultsChangedEventArgs<University> args)
    {
        try
        {
            if(args.Exception != null)
            {
                await HandleExceptionAsync(args.Exception);
            }
            else if(args.DataEntities != null)
            {
                Universities = args.DataEntities;
            }
        }
        catch(Exception exc)
        {
            await HandleExceptionAsync(exc);
        }
    }

    public override async Task OnFirstAppearingAsync(bool refresh)
    {
        await RefreshUniversitiesAsync();
    }

    [RelayCommand]
    async Task SelectUniversity(University university)
    {
        UniversitySelected?.Invoke(university);
        await Dismiss();
    }

    [RelayCommand]
    Task EditUniversity(University university)
    {
        return navigationService.PushAsync<UniversityViewModel>(true, (vm) => {
            vm.Id = university.Id;
        });
    }

    [RelayCommand]
    async Task DeleteUniversity(University university)
    {
        var confirmed = await alertService.ShowMessage("Confirm", $"Are you sure you want to permanently delete {university.Name} ?", "Yes", "No");
        if(confirmed)
        {
            var success = await universityRepository.DeleteAsync(university);
            if(!success)
            {
                await alertService.ShowMessage(string.Empty, $"Error deleting university {university.Name}", "OK");
            }
        }
    }

    [RelayCommand]
    async Task TextChangedAsync(TextChangedEventArgs args)
    {
        await RefreshUniversitiesAsync();
    }

    [RelayCommand]
    Task Add()
    {
        return navigationService.PushAsync<UniversityViewModel>(true, (vm) => {
            vm.UniversitySaved = (name) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Name = name;
                });
            };
        });
    }

    // [RelayCommand]
    // async Task SearchAsync()
    // {
    //     if(!string.IsNullOrEmpty(Name))
    //     {
    //         try
    //         {
    //             Universities = await universityRepository.SearchByName(Name, Country);
    //         }
    //         catch(Exception exc)
    //         {
    //             await alertService.ShowMessage(exc.GetType().FullName!, exc.Message, "OK");
    //         }
    //     }
    // }
}
