using Couchbase.Lite;

namespace CouchbaseMauiPOC.Infrastructure.Sources;

public interface IDataSource : IDisposable
{
    string DatabaseName {get;}
    Database? Database {get;}

    Task<Database> GetDatabaseAsync();
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
