using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CouchbaseMauiPOC.Models;
using CouchbaseMauiPOC.Repositories;
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

    public Action<string>? UniversitySelected {get; set;}

    public UniversitiesViewModel(INavigationService navigationService, IUniversityRepository universityRepository, IAlertService alertService)
     : base(navigationService)
     {
        this.universityRepository = universityRepository;
        this.alertService = alertService;
    }

    [RelayCommand]
    Task SelectUniversity(University university)
    {
        UniversitySelected?.Invoke(university.Name!);
        return Dismiss();
    }

    [RelayCommand]
    async Task SearchAsync()
    {
        if(!string.IsNullOrEmpty(Name))
        {
            try
            {
                Universities = await universityRepository.SearchByName(Name, Country);
            }
            catch(Exception exc)
            {
                await alertService.ShowMessage(exc.GetType().FullName!, exc.Message, "OK");
            }
        }
    }
}
