using CommunityToolkit.Mvvm.ComponentModel;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    protected readonly IAlertService alertService;

    [ObservableProperty]
    bool isBusy;

    protected BaseViewModel(IAlertService alertService)
    {
        this.alertService = alertService;
    }

    public virtual Task OnFirstAppearingAsync(bool refresh) => Task.FromResult(true);

    protected virtual async Task HandleExceptionAsync(Exception exc)
    {
        await alertService.ShowMessage(exc.GetType().Name, exc.Message, "OK");
    }
}
