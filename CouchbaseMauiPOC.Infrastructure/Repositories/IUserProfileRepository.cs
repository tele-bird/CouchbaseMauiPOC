using CouchbaseMauiPOC.Infrastructure.Events;
using CouchbaseMauiPOC.Infrastructure.Models;

namespace CouchbaseMauiPOC.Infrastructure.Repositories;

public interface IUserProfileRepository : IBaseRepository
{
    event UserProfileQueryResultChangedEvent? UserProfileResultChanged;
    // Task<UserProfile?> GetLocalAsync(string userProfileId);
    Task<bool> SaveAsync(UserProfile userProfile);
}
