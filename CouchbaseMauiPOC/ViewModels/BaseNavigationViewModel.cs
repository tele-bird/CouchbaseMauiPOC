using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.ViewModels;

public abstract class BaseNavigationViewModel : BaseViewModel
{
    protected readonly INavigationService navigationService;

    protected BaseNavigationViewModel(INavigationService navigationService, IAlertService alertService)
        : base(alertService)
    {
        this.navigationService = navigationService;
    }

    public Task Dismiss() => navigationService.PopAsync();
}
