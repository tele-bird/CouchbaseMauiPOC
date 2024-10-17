using System.Diagnostics;
using Couchbase.Lite;
using Couchbase.Lite.Query;
using CouchbaseMauiPOC.Models;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.Repositories;

public sealed class UserProfileRepository : BaseRepository, IUserProfileRepository
{
    private ListenerToken? userQueryToken;
    private ListenerToken? UserQueryToken
    {
        get
        {
            return userQueryToken;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(databaseManager.Database, nameof(databaseManager.Database));
            if(userQueryToken.HasValue)
            {
                databaseManager.Database.GetDefaultCollection().RemoveChangeListener(userQueryToken.Value);
            }
            userQueryToken = value;
        }
    }


    public event UserProfileQueryResultChangedEvent? UserProfileResultChanged;

    public UserProfileRepository(IDatabaseSeedService databaseSeedService)
        : base(databaseSeedService, "userprofile")
    {
    }

    private List<UserProfile> ExtractResults(List<Result> results)
    {
        var userProfiles = new List<UserProfile>();
        foreach (var result in results)
        {
            // var jsonResult = result.ToJSON();
            // Trace.WriteLine($"result[{rowNum}]: {jsonResult}");
            var dictionary = result.GetDictionary("_default");
            ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));
            // var rowDebugString = dictionary.ToJSON();
            // Trace.WriteLine($"result[{rowNum}]: {rowDebugString}");
            var userProfile = new UserProfile
            {
                Id = result.GetString("id"),
                Name = dictionary.GetString("name"),
                Email = dictionary.GetString("email"),
                Address = dictionary.GetString("address"),
                ImageData = dictionary.GetBlob("imageData")?.Content,
                Description = dictionary.GetString("description"),
                University = dictionary.GetString("university")
            };

            userProfiles.Add(userProfile);
        }

        return userProfiles;
    }

    // public async Task<UserProfile?> GetLocalAsync(string userProfileId)
    // {
    //     UserProfile? userProfile = null;
    //     try
    //     {
    //         var database = await GetDatabaseAsync();
    //         if (database != null)
    //         {
    //             var document = database.GetDefaultCollection().GetDocument(userProfileId);
    //             if (document != null)
    //             {
    //                 userProfile = new UserProfile
    //                 {
    //                     Id = document.Id,
    //                     Name = document.GetString("name"),
    //                     Email = document.GetString("email"),
    //                     Address = document.GetString("address"),
    //                     ImageData = document.GetBlob("imageData")?.Content,
    //                     Description = document.GetString("description"),
    //                     University = document.GetString("university")
    //                 };
    //             }
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Trace.WriteLine($"UserProfileRepository Exception: {ex.Message}");
    //     }
        
    //     return userProfile;
    //  }

    public async Task GetAsync(string userProfileId)
    {
        var database = await GetDatabaseAsync();
        ArgumentNullException.ThrowIfNull(database, nameof(database));
        UserQueryToken = QueryBuilder
            .Select(SelectResult.Expression(Meta.ID), SelectResult.All())
            .From(DataSource.Collection(database.GetDefaultCollection()))
            // .Where(Expression.Property("name").EqualTo(Expression.String("Phil")))
            .Where(Meta.ID.EqualTo(Expression.String(userProfileId)))
                .AddChangeListener(HandleQueryResultsChanged);
     }

    private void HandleQueryResultsChanged(object? sender, QueryChangedEventArgs e)
    {
        var resultsChangedEventArgs = new QueryResultChangedEventArgs<UserProfile>();
        if(e.Error != null)
        {
            resultsChangedEventArgs.Exception = e.Error;
        }
        else
        {
            try
            {
                var resultsList = e.Results.AllResults();
                Trace.WriteLine($"{nameof(UserProfileRepository)}.{nameof(HandleQueryResultsChanged)} >> got {resultsList.Count} results");
                var userProfiles = ExtractResults(resultsList);
                if(userProfiles.Count > 1)
                {
                    throw new Exception($"Unexpected scenario: query returned {userProfiles.Count} entities.");
                }
                resultsChangedEventArgs.DataEntity = userProfiles[0];
            }
            catch(Exception exc)
            {
                resultsChangedEventArgs.Exception = exc;
            }
        }

        UserProfileResultChanged?.Invoke(resultsChangedEventArgs);
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
                mutableDocument.SetString("type", userProfile.Type);
                
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
                AppInstance.User!.Username!,
                AppInstance.User!.Password!,
                new string[] { AppInstance.User!.Username! }
            );
        });
    }

    public override void Dispose()
    {
        UserQueryToken = null;
        base.Dispose();
    }
}
