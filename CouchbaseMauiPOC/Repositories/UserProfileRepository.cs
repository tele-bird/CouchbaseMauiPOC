using System.Diagnostics;
using Couchbase.Lite;
using Couchbase.Lite.Query;
using CouchbaseMauiPOC.Models;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.Repositories;

public sealed class UserProfileRepository : BaseRepository, IUserProfileRepository
{
    private ListenerToken? userQueryToken;

    public event UserProfileChangedEvent? UserProfileChanged;

    public UserProfileRepository(IDatabaseSeedService databaseSeedService)
        : base(databaseSeedService, "userprofile")
        {
        }

    public async Task<UserProfile?> GetLocalAsync(string userProfileId)
    {
        UserProfile? userProfile = null;
        try
        {
            var database = await GetDatabaseAsync();
            if (database != null)
            {
                var document = database.GetDefaultCollection().GetDocument(userProfileId);
                if (document != null)
                {
                    userProfile = new UserProfile
                    {
                        Id = document.Id,
                        Name = document.GetString("name"),
                        Email = document.GetString("email"),
                        Address = document.GetString("address"),
                        ImageData = document.GetBlob("imageData")?.Content,
                        University = document.GetString("university")
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"UserProfileRepository Exception: {ex.Message}");
        }
        
        return userProfile;
     }

    public async Task GetAsync(string userProfileId)
    {
        try
        {
            var database = await GetDatabaseAsync();
            if (database != null)
            {
                Trace.WriteLine($"querying for ID = {userProfileId}");
                userQueryToken = QueryBuilder
                    .Select(SelectResult.All())
                    .From(DataSource.Collection(database.GetDefaultCollection()))
                    // .Where(Expression.Property("name").EqualTo(Expression.String("Phil")))
                    .Where(Meta.ID.EqualTo(Expression.String(userProfileId)))
                        .AddChangeListener(HandleUserQueryChanged);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"UserProfileRepository Exception: {ex.Message}");
        }
     }

    private void HandleUserQueryChanged(object? sender, QueryChangedEventArgs e)
    {
        if(e.Error != null)
        {
            Trace.WriteLine($"Live query change listener received error: {e.Error.GetType().Name}: {e.Error.Message}");
        }
        else if (e?.Results != null)
        {
            var resultsList = e.Results.AllResults();
            Trace.WriteLine($"Live query change listener received {resultsList.Count} results.");
            int i = 1;
            foreach (var result in resultsList)
            {
                Trace.WriteLine($"Result #{i} has keys: {string.Join(',',result.Keys)}");
                foreach(var key in result.Keys)
                {
                    var value = result[key];
                    Trace.WriteLine($"key #{i} has name: {key} and value: {value}");
                    var dict = result[key].Dictionary;
                    if(dict != null)
                    {
                        OutputResults(dict);
                    }
                }
                var dictionary = result.GetDictionary("_default");
                if (dictionary != null)
                {
                    Trace.WriteLine($"Result #{i} has a userprofile dictionary");
                    var userProfile = new UserProfile
                    {
                        Name = dictionary.GetString("name"),
                        Email = dictionary.GetString("email"),
                        Address = dictionary.GetString("address"),
                        University = dictionary.GetString("university"),
                        ImageData = dictionary.GetBlob("imageData")?.Content
                    };

                    UserProfileChanged?.Invoke(new UserProfileChangedEventArgs(userProfile));
                }
                else
                {
                    Trace.WriteLine($"Result #{i} has NO userprofile dictionary");
                }
                ++i;
            }
        }
    }

    private void OutputResults(DictionaryObject? result)
    {
        if(result != null)
        {
            int i = 1;
            foreach(var key in result.Keys)
            {
                var value = result[key];
                Trace.WriteLine($"key #{i} has name: {key} and value: {value}");
                var dict = result[key].Dictionary;
                if(dict != null)
                {
                    OutputResults(dict);
                }
                ++i;
            }
        }
    }

    public async Task<bool> SaveAsync(UserProfile userProfile)
     {
        try
        {
            if (userProfile != null)
            {
                var mutableDocument = new MutableDocument(userProfile.Id);
                mutableDocument.SetString("name", userProfile.Name);
                mutableDocument.SetString("email", userProfile.Email);
                mutableDocument.SetString("address", userProfile.Address);
                mutableDocument.SetString("university", userProfile.University);
                mutableDocument.SetString("type", "user");
                
                if (userProfile.ImageData != null)
                {
                    mutableDocument.SetBlob("imageData", new Blob("image/jpeg", userProfile.ImageData));
                }
                
                var database = await GetDatabaseAsync();
                database.GetDefaultCollection().Save(mutableDocument);
                return true;
            }
        }
        catch(Exception ex)
        {
            Trace.WriteLine($"UserProfileRepository Exception: {ex.Message}");
        }
        
        return false;
      }

    public Task StartReplicationForCurrentUser()
    {
        return Task.Run(async () => 
        {
            await databaseManager.StartReplicationAsync(
                AppInstance.User!.Username,
                AppInstance.User!.Password,
                new string[] { AppInstance.User!.Username! }
            );
        });
    }

    public override void Dispose()
    {
        if(userQueryToken.HasValue && databaseManager.Database != null)
        {
            databaseManager.Database.GetDefaultCollection().RemoveChangeListener(userQueryToken.Value);
            userQueryToken = null;
        }

        base.Dispose();
    }
}
