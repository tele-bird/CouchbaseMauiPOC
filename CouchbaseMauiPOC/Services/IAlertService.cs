using System;

namespace CouchbaseMauiPOC.Services;

public interface IAlertService
{
    Task ShowMessage(string title, string message, string cancel);
    Task<bool> ShowMessage(string titel, string message, string accept, string cancel);
}
