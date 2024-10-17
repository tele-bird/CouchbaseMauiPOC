using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC.Pages;

public partial class UniversityPage : BaseContentPage<UniversityViewModel>
{
	public UniversityPage(UniversityViewModel universityViewModel)
		: base(universityViewModel)
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		IdEntry.Loaded += delegate { IdEntry.Focus(); };
    }
}