using CouchbaseMauiPOC.Models;

namespace CouchbaseMauiPOC.Repositories;

public class UserProfileChangedEventArgs : EventArgs
{
    public UserProfile UserProfile { get; private set;}

    public UserProfileChangedEventArgs(UserProfile userProfile)
    {
        UserProfile = userProfile;
    }
}

public delegate void UserProfileChangedEvent(UserProfileChangedEventArgs args);
