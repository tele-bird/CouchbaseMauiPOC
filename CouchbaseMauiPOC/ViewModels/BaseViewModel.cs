using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CouchbaseMauiPOC.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    bool isBusy;
}
