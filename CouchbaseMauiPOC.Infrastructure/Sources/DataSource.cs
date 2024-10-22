using System.Diagnostics;
using Couchbase.Lite;

namespace CouchbaseMauiPOC.Infrastructure.Sources;

public abstract class DataSource : IDataSource
{
    public string DatabaseName { get; private set; }
    public Database? Database {get; protected set;}

    protected DataSource(string databaseName)
    {
        this.DatabaseName = databaseName;
    }

    public abstract Task<Database> GetDatabaseAsync();

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        Trace.WriteLine($"{GetType().Name}.{nameof(StartAsync)} >>");
        return Task.CompletedTask;
    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        Trace.WriteLine($"{GetType().Name}.{nameof(StopAsync)} >>");
        return Task.CompletedTask;
    }

    public abstract void Dispose();

    protected virtual void TraceDocumentChange(object? sender, DocumentChangedEventArgs e)
    {
        Trace.WriteLine($"{GetType().Name}.{nameof(TraceDocumentChange)}: Document with ID {e.DocumentID} changed. Collection: {e.Collection.Name}");
    }

    protected virtual void TraceDatabaseChange(object? sender, CollectionChangedEventArgs e)
    {
        var documentIdsList = string.Join(',', e.DocumentIDs);
        Trace.WriteLine($"{GetType().Name}.{nameof(TraceDocumentChange)}: {e.Database.Name} database changed {e.Collection.Count} items: [{documentIdsList}]");
    }
}
