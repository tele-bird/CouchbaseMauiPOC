using CouchbaseMauiPOC.Pages;
using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC.Services;

public interface INavigationService
{
    public void Register<TBaseViewModel, TBaseContentPage>()
        where TBaseViewModel : BaseViewModel
        where TBaseContentPage : BaseContentPage;

    Task PopAsync(bool animate = false);

    Task PushAsync<T>(bool animated = false, Action<T>? initAction = null)
        where T : BaseViewModel;

    Task PopToRootAsync(bool animate = false);
}
