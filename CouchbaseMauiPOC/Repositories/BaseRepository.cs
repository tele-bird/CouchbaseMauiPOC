using System.Diagnostics;
using Couchbase.Lite;

namespace CouchbaseMauiPOC.Repositories;

public abstract class BaseRepository : IBaseRepository
{
    private readonly IServiceProvider serviceProvider;
    private readonly string databaseName;
    private Database? database;
    private ListenerToken? databaseChangeListenerToken;


    public string? Path => database!.Path;

    protected BaseRepository(IServiceProvider serviceProvider, string databaseName)
    {
        this.serviceProvider = serviceProvider;
        this.databaseName = databaseName;
    }

    protected virtual async Task<Database> GetDatabaseAsync()
    {
        if(database == null)
        {
            DatabaseManager databaseManager = new DatabaseManager(serviceProvider, databaseName);
            database = await databaseManager.GetDatabaseAsync();
            databaseChangeListenerToken = database.GetDefaultCollection().AddChangeListener(TraceDatabaseChange);
        }

        return database;
    }

    private void TraceDatabaseChange(object? sender, CollectionChangedEventArgs e)
    {
        var documentIdsList = string.Join(", ", e.DocumentIDs);
        Trace.WriteLine($"Database {e.Database.Path} changed {e.Collection.Count} items: [{documentIdsList}]");
    }

    public virtual void Dispose()
    {
        if(database != null)
        {
            if(databaseChangeListenerToken.HasValue)
            {
                database.GetDefaultCollection().RemoveChangeListener(databaseChangeListenerToken.Value);
                databaseChangeListenerToken = null;
            }
            
            database.Close();
            database = null;
        }
    }
}
