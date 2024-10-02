using System;

namespace CouchbaseMauiPOC.Services;

public class AlertService : IAlertService
{
    public Task ShowMessage(string title, string message, string cancel)
    {
        return Application.Current!.MainPage!.DisplayAlert(title, message, cancel);
    }
}
