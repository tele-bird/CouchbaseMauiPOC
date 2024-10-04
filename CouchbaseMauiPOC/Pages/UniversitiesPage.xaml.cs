using CouchbaseMauiPOC.ViewModels;

namespace CouchbaseMauiPOC.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class UniversitiesPage : BaseContentPage<UniversitiesViewModel>
{
	public UniversitiesPage(UniversitiesViewModel universitiesViewModel)
		: base(universitiesViewModel)
	{
		InitializeComponent();
	}
}