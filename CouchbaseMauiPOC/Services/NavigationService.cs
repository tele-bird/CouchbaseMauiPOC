using CouchbaseMauiPOC.Pages;
using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC.Services;

public class NavigationService : INavigationService
{
    INavigation StackNavigation => Application.Current!.MainPage!.Navigation;

    // View model to view lookup - making the assumption that view model to view will always be 1:1
    private readonly Dictionary<Type, Type> _viewModelViewDictionary = new Dictionary<Type, Type>();
    private readonly IServiceProvider serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void Register<TBaseViewModel, TBaseContentPage>()
        where TBaseViewModel : BaseViewModel
        where TBaseContentPage : BaseContentPage
    {
        if (!_viewModelViewDictionary.ContainsKey(typeof(TBaseViewModel)))
        {
            _viewModelViewDictionary.Add(typeof(TBaseViewModel), typeof(TBaseContentPage));
        }
    }

    public Task PopAsync(bool animate = false) => StackNavigation.PopAsync(animate);

    public Task PopToRootAsync(bool animate = false) => StackNavigation.PopToRootAsync(animate);

    public Task PushAsync<TViewModel>(bool animate = false, Action<TViewModel>? initAction = null) where TViewModel : BaseViewModel
    {
        var viewType = _viewModelViewDictionary[typeof(TViewModel)];
        var view = (BaseContentPage<TViewModel>)serviceProvider.GetRequiredService(viewType);
        initAction?.Invoke(view.ViewModel);
        return StackNavigation.PushAsync(view, animate);
    }
}
