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

    public string? Id {get; set;}

    [ObservableProperty]
    private University university;

    public Action<string>? UniversitySaved {get; set;}

    public UniversityViewModel(INavigationService navigationService, IUniversityRepository universityRepository, IAlertService alertService)
        : base(navigationService, alertService)
    {
        this.universityRepository = universityRepository;
        this.University = new University();
        universityRepository.UniversityResultChanged += UpdateUniversityResultAsync;
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
            await HandleExceptionAsync(exc);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void UpdateUniversityResultAsync(QueryResultChangedEventArgs<University> args)
    {
        try
        {
            if(args.Exception != null)
            {
                await HandleExceptionAsync(args.Exception);
            }
            else if(args.DataEntity != null)
            {
                University = args.DataEntity;
            }
        }
        catch(Exception exc)
        {
            await HandleExceptionAsync(exc);
        }
    }

    [RelayCommand]
    async Task Save()
    {
        try
        {
            if(string.IsNullOrEmpty(University.Name) || string.IsNullOrEmpty(University.Country))
            {
                throw new ArgumentException("Both name and country are required.");
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
                    throw new Exception($"Error updating university.");
                }
            }
        }
        catch(Exception exc)
        {
            await HandleExceptionAsync(exc);
        }
    }
}
