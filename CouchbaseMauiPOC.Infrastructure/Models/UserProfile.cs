using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CouchbaseMauiPOC.Infrastructure.Models;

public partial class UserProfile : DataEntity
{
    public override string Type {get => "user";}

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private string? email;

    [ObservableProperty]
    private string? address;

    [ObservableProperty]
    private byte[]? imageData;

    [ObservableProperty]
    private string? description;

    [ObservableProperty]
    private string? universityId;
}
