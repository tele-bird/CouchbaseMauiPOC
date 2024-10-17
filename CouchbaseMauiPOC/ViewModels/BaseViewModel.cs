using CommunityToolkit.Mvvm.ComponentModel;

namespace CouchbaseMauiPOC.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    bool isBusy;

    public virtual Task OnFirstAppearingAsync(bool refresh) => Task.FromResult(true);
}
