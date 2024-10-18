using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CouchbaseMauiPOC.Infrastructure.Events;
using CouchbaseMauiPOC.Infrastructure.Models;
using CouchbaseMauiPOC.Infrastructure.Repositories;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.ViewModels;

public partial class UniversitiesViewModel : BaseNavigationViewModel
{
    private readonly IUniversityRepository universityRepository;
    private readonly IAlertService alertService;

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
     : base(navigationService)
     {
        this.universityRepository = universityRepository;
        this.alertService = alertService;
    }

    private async Task RefreshUniversitiesAsync()
    {
        IsBusy = true;
        try
        {
            universityRepository.UniversityResultsChanged += UpdateUniversityResults;
            await universityRepository.StartsWith(Name, Country);
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

    private void UpdateUniversityResults(QueryResultsChangedEventArgs<University> args)
    {
        try
        {
            if(args.Exception != null)
            {
                alertService.ShowMessage(args.Exception.GetType().Name, args.Exception.Message, "OK");
            }
            else if(args.DataEntities != null)
            {
                Universities = args.DataEntities;
            }
        }
        catch(Exception exc)
        {
            alertService.ShowMessage(exc.GetType().Name, exc.Message, "OK");
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
