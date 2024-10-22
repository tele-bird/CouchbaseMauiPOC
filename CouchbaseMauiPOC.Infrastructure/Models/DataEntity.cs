using CommunityToolkit.Mvvm.ComponentModel;

namespace CouchbaseMauiPOC.Infrastructure.Models;

public abstract class DataEntity : ObservableObject
{
    public abstract string Type {get;}
    public string? Id {get; set;}
}
