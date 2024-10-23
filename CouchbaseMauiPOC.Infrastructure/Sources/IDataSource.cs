using Couchbase.Lite;

namespace CouchbaseMauiPOC.Infrastructure.Sources;

public interface IDataSource : IDisposable
{
    string DatabaseName {get;}
    Database? Database {get;}

    Task<Database> GetDatabaseAsync();
    void Pause(CancellationToken cancellationToken);
    void Resume(CancellationToken cancellationToken);
}
