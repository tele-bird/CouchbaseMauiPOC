using System.Diagnostics;
using Couchbase.Lite;
using CouchbaseMauiPOC.Models;

namespace CouchbaseMauiPOC.Repositories;

public sealed class UserProfileRepository : BaseRepository<UserProfile, string>, IUserProfileRepository
{
    DatabaseConfiguration? databaseConfig;
    protected override DatabaseConfiguration DatabaseConfig
    {
        get
        {
            if(databaseConfig == null)
            {
                if(AppInstance.User?.Username == null)
                {
                    throw new Exception($"Repository Exception: a valid user is required.");
                }

                databaseConfig = new DatabaseConfiguration
                {
                    Directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppInstance.User.Username)
                };
            }

            return databaseConfig;
        }
    }

    public UserProfileRepository()
        : base("userprofile")
        {

        }

    public override UserProfile? Get(string userProfileId)
    {
        UserProfile? userProfile = null;

        try
        {
            var document = Database.GetDefaultCollection().GetDocument(userProfileId);

            if(document != null)
            {
                userProfile = new UserProfile
                {
                    Id = document.Id,
                    Name = document.GetString("Name"),
                    Email = document.GetString("Email"),
                    Address = document.GetString("Address"),
                    ImageData = document.GetBlob("ImageData")?.Content
                };
            }
        }
        catch(Exception exc)
        {
            Trace.WriteLine($"UserProfileRepository Exception: {exc.Message}");
        }

        return userProfile;
    }

    public override bool Save(UserProfile userProfile)
    {
        try
        {
            if(userProfile != null)
            {
                var mutableDocument = new MutableDocument(userProfile.Id);
                mutableDocument.SetString("Name", userProfile.Name);
                mutableDocument.SetString("Email", userProfile.Email);
                mutableDocument.SetString("Address", userProfile.Address);
                mutableDocument.SetString("type", "user");
                if(userProfile.ImageData != null)
                {
                    mutableDocument.SetBlob("ImageData", new Blob("image/jpeg", userProfile.ImageData));
                }

                Database.GetDefaultCollection().Save(mutableDocument);

                return true;
            }
        }
        catch(Exception exc)
        {
            Trace.WriteLine($"UserProfileRepository Exception: {exc.Message}");
        }

        return false;
    }
}
