using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CouchbaseMauiPOC.Infrastructure.Events;
using CouchbaseMauiPOC.Infrastructure.Models;
using CouchbaseMauiPOC.Infrastructure.Repositories;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.ViewModels;

public partial class UniversityViewModel : BaseNavigationViewModel
{
    private readonly IUniversityRepository universityRepository;
    private readonly IAlertService alertService;

    public string? Id {get; set;}

    [ObservableProperty]
    private University university;

    public Action<string>? UniversitySaved {get; set;}

    public UniversityViewModel(INavigationService navigationService, IUniversityRepository universityRepository, IAlertService alertService)
        : base(navigationService)
    {
        this.universityRepository = universityRepository;
        this.alertService = alertService;
        this.University = new University();
        universityRepository.UniversityResultChanged += UpdateUniversityResult;
    }

    public override async Task OnFirstAppearingAsync(bool refresh)
    {
        await LoadUniversityAsync();
    }

    private async Task LoadUniversityAsync()
    {
        IsBusy = true;
        try
        {
            if(!string.IsNullOrEmpty(Id))
            {
                await universityRepository.GetAsync(Id);
            }
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

    private async void UpdateUniversityResult(QueryResultChangedEventArgs<University> args)
    {
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
        if(string.IsNullOrEmpty(University.Name) || string.IsNullOrEmpty(University.Country))
        {
            await alertService.ShowMessage("Missing", "Both name and country are required.", "OK");
        }
        else
        {
            var newId = await universityRepository.SaveAsync(University).ConfigureAwait(false);

            if(newId != null)
            {
                University.Id = newId;
                await Dismiss();
            }
            else
            {
                await alertService.ShowMessage(string.Empty, $"Error updating university to {universityRepository.Path}", "OK");
            }
        }
    }
}
