using Couchbase.Lite;

namespace CouchbaseMauiPOC.Infrastructure.Repositories;

public abstract class BaseRepository : IBaseRepository
{
    protected readonly Sources.DataSource DataSource;

    public string? Path => DataSource.Database?.Path;

    protected BaseRepository(Sources.DataSource dataSource)
    {
        this.DataSource = dataSource;
    }

    protected virtual Task<Database> GetDatabaseAsync()
    {
        return DataSource.GetDatabaseAsync();
    }

    public abstract Task GetAsync(string id);


    public virtual void Dispose()
    {
    }
}
