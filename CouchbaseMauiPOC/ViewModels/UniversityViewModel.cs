using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CouchbaseMauiPOC.Models;
using CouchbaseMauiPOC.Repositories;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.ViewModels;

public partial class UniversityViewModel : BaseNavigationViewModel
{
    private readonly IUniversityRepository universityRepository;
    private readonly IAlertService alertService;

    [ObservableProperty]
    string? id;
    [ObservableProperty]
    string? name;
    [ObservableProperty]
    string? country;
    [ObservableProperty]
    string? alphaTwoCode;

    public Action<string>? UniversitySaved {get; set;}

    public UniversityViewModel(INavigationService navigationService, IUniversityRepository universityRepository, IAlertService alertService)
        : base(navigationService)
    {
        this.universityRepository = universityRepository;
        this.alertService = alertService;
    }

    public override async Task OnFirstAppearingAsync(bool refresh)
    {
        if(!string.IsNullOrEmpty(Id))
        {
            var universityToEdit = await universityRepository.GetLocalAsync(Id);
            if(universityToEdit == null)
            {
                await alertService.ShowMessage("Error", $"Whoa, I didn't find a University with id: {Id}", "Close");
                _ = Dismiss();
            }
            else
            {
                this.Id = universityToEdit.Id;
                this.Name = universityToEdit.Name;
                this.Country = universityToEdit.Country;
                this.AlphaTwoCode = universityToEdit.AlphaTwoCode;
            }
        }
    }

    [RelayCommand]
    async Task Save()
    {
        if(string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Country))
        {
            await alertService.ShowMessage("Missing", "Both name and country are required.", "OK");
        }
        else
        {
            var university = new University
            {
                Id = this.Id,
                Name = this.Name,
                Country = this.Country,
                AlphaTwoCode = this.AlphaTwoCode
            };

            var newId = await universityRepository.SaveAsync(university).ConfigureAwait(false);

            if(newId != null)
            {
                this.Id = newId;
                await Dismiss();
            }
            else
            {
                await alertService.ShowMessage(string.Empty, $"Error updating university to {universityRepository.Path}", "OK");
            }
        }
    }
}
