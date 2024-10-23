using Couchbase.Lite;

namespace CouchbaseMauiPOC.Infrastructure.Repositories;

public abstract class BaseRepository : IBaseRepository
{
    protected readonly Sources.IDataSource DataSource;

    public string? Path => DataSource.Database!.Name;

    protected BaseRepository(Sources.IDataSource dataSource)
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
