using CouchbaseMauiPOC.Models;

namespace CouchbaseMauiPOC.Repositories;

public interface IUserProfileRepository : IBaseRepository
{
    Task<UserProfile?> GetAsync(string userProfileId);
    Task<bool> SaveAsync(UserProfile userProfile);
}
