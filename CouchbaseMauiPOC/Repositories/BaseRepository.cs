using System.Diagnostics;
using Couchbase.Lite;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.Repositories;

public abstract class BaseRepository : IBaseRepository
{
    private readonly IDatabaseSeedService databaseSeedService;
    private readonly string databaseName;
    private Database? database;
    private ListenerToken? databaseChangeListenerToken;


    public string? Path => database!.Path;

    protected BaseRepository(IDatabaseSeedService databaseSeedService, string databaseName)
    {
        this.databaseSeedService = databaseSeedService;
        this.databaseName = databaseName;
    }

    protected virtual async Task<Database> GetDatabaseAsync()
    {
        Trace.WriteLine($"{GetType().Name}.{nameof(GetDatabaseAsync)} >>");
        if(database == null)
        {
            Trace.WriteLine($"{GetType().Name}.{nameof(GetDatabaseAsync)} - CREATING {databaseName} DATABASE");
            DatabaseManager databaseManager = new DatabaseManager(databaseSeedService, databaseName);
            database = await databaseManager.GetDatabaseAsync();
            databaseChangeListenerToken = database.GetDefaultCollection().AddChangeListener(TraceDatabaseChange);
            Trace.WriteLine($"{GetType().Name}.{nameof(GetDatabaseAsync)} - DATABASE {databaseName} CREATED");
        }

        Trace.WriteLine($"{GetType().Name}.{nameof(GetDatabaseAsync)} <<");
        return database;
    }

    private void TraceDatabaseChange(object? sender, CollectionChangedEventArgs e)
    {
        var documentIdsList = string.Join(", ", e.DocumentIDs);
        Trace.WriteLine($"Database {e.Database.Path} changed {e.Collection.Count} items: [{documentIdsList}]");
    }

    public virtual void Dispose()
    {
        Trace.WriteLine($"{GetType().Name}.{nameof(Dispose)} >>");

        if(database != null)
        {
            Trace.WriteLine($"{GetType().Name}.{nameof(Dispose)} DISPOSING");
            if(databaseChangeListenerToken.HasValue)
            {
                database.GetDefaultCollection().RemoveChangeListener(databaseChangeListenerToken.Value);
                databaseChangeListenerToken = null;
            }

            database.Close();
            database = null;
        }

        Trace.WriteLine($"{GetType().Name}.{nameof(Dispose)} <<");
    }
}
