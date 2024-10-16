using System;

namespace CouchbaseMauiPOC.Services;

public class AlertService : IAlertService
{
    public Task ShowMessage(string title, string message, string cancel)
    {
        return Application.Current!.MainPage!.DisplayAlert(title, message, cancel);
    }

    public Task<bool> ShowMessage(string titel, string message, string accept, string cancel)
    {
        return Application.Current!.MainPage!.DisplayAlert(titel, message, accept, cancel);
    }
}
