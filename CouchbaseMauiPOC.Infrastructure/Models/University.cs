using CommunityToolkit.Mvvm.ComponentModel;

namespace CouchbaseMauiPOC.Infrastructure.Models;

public partial class University : DataEntity
{
    public override string Type {get => "university";}

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private string? country;

    [ObservableProperty]
    private string? alphaTwoCode;
}
