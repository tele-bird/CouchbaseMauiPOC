using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC.Pages;

public abstract class BaseContentPage : ContentPage
{
}

public abstract class BaseContentPage<TViewModel> : BaseContentPage where TViewModel : BaseViewModel
{
    public TViewModel ViewModel => (TViewModel)BindingContext;

	protected BaseContentPage(TViewModel viewModel)
	{
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		Task.Run(async () => 
		{
			await ViewModel!.LoadAsync(true);
		});
    }
}