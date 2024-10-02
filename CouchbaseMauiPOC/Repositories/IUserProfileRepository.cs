using CouchbaseMauiPOC.Models;

namespace CouchbaseMauiPOC.Repositories;

public interface IUserProfileRepository : IRepository<UserProfile, string>
{
    new UserProfile? Get(string userProfileId);
    new bool Save(UserProfile userProfile);
}
