using CouchbaseMauiPOC.Models;

namespace CouchbaseMauiPOC.Repositories;

public interface IUserProfileRepository : IBaseRepository
{
    event UserProfileQueryResultsChangedEvent? UserProfileResultsChanged;
    Task GetAsync(string userProfileId);
    Task<UserProfile?> GetLocalAsync(string userProfileId);
    Task<bool> SaveAsync(UserProfile userProfile);
    Task StartReplicationForCurrentUser();
}
