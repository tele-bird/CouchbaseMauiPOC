using CouchbaseMauiPOC.Infrastructure.Events;
using CouchbaseMauiPOC.Infrastructure.Models;

namespace CouchbaseMauiPOC.Infrastructure.Repositories;

public interface IUserProfileRepository : IBaseRepository
{
    event UserProfileQueryResultChangedEvent? UserProfileResultChanged;
    Task GetAsync(string userProfileId);
    // Task<UserProfile?> GetLocalAsync(string userProfileId);
    Task<bool> SaveAsync(UserProfile userProfile);
    Task StartReplicationForCurrentUser(string username, string password, string[] channels);
}
