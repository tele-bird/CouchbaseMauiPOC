using Couchbase.Lite;
using CouchbaseMauiPOC.Infrastructure.Services;

namespace CouchbaseMauiPOC.Infrastructure.Repositories;

public abstract class BaseRepository : IBaseRepository
{
    protected DatabaseManager databaseManager;

    public string? Path => databaseManager.Database?.Path;

    protected BaseRepository(IDatabaseSeedService databaseSeedService, string databaseName)
    {
        databaseManager = new DatabaseManager(databaseSeedService, databaseName);
    }

    protected virtual Task<Database> GetDatabaseAsync()
{
        return databaseManager.GetDatabaseAsync();
    }

    public virtual void Dispose()
    {
        databaseManager.Dispose();
    }
}
