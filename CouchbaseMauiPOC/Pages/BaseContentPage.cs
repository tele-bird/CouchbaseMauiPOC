using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC.Pages;

public abstract class BaseContentPage : ContentPage
{
}

public abstract class BaseContentPage<TViewModel> : BaseContentPage where TViewModel : BaseViewModel
{
	private bool hasAppeared;

    public TViewModel ViewModel => (TViewModel)BindingContext;

	protected BaseContentPage(TViewModel viewModel)
	{
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		if(!hasAppeared)
		{
			Task.Run(async () => 
			{
				await ViewModel!.OnFirstAppearingAsync(true);
			});

			hasAppeared = true;
		}
    }
}