using System.Diagnostics;
using Couchbase.Lite;
using Couchbase.Lite.DI;
using Couchbase.Lite.Query;
using CouchbaseMauiPOC.Infrastructure.Models;
using CouchbaseMauiPOC.Infrastructure.Services;

namespace CouchbaseMauiPOC.Infrastructure.Sources;

public class CouchbaseDataSource : IDataSource
{
    private readonly DataSourceConfiguration dataSourceConfiguration;
    private readonly IDatabaseSeedService? databaseSeedService;
    private readonly Couchbase.Lite.DatabaseConfiguration databaseConfiguration;

    public Database? Database {get; protected set;}
    public string DatabaseName => dataSourceConfiguration.DatabaseConfiguration!.DatabaseName!;
    private DataSyncService? dataSyncService;
    private ListenerToken? databaseListenerToken;
    private ListenerToken? documentListenerToken;

    public CouchbaseDataSource(DataSourceConfiguration dataSourceConfiguration, IDatabaseSeedService? databaseSeedService = null)
    {
        this.dataSourceConfiguration = dataSourceConfiguration;
        this.databaseSeedService = databaseSeedService;
        var defaultDirectoryResolver = Service.GetInstance<IDefaultDirectoryResolver>();
        ArgumentNullException.ThrowIfNull(defaultDirectoryResolver);
        databaseConfiguration = new Couchbase.Lite.DatabaseConfiguration()
        {
            Directory = defaultDirectoryResolver.DefaultDirectory()
        };
    }

    public void Pause(CancellationToken cancellationToken)
    {
        Trace.WriteLine($"{GetType().Name}({dataSourceConfiguration.DatabaseConfiguration!.DatabaseName}).{nameof(Pause)} >>");
        ArgumentNullException.ThrowIfNull(Database);
        dataSyncService?.Stop(cancellationToken);
        Trace.WriteLine($"{GetType().Name}({dataSourceConfiguration.DatabaseConfiguration!.DatabaseName}).{nameof(Pause)} <<");
    }

    public void Resume(CancellationToken cancellationToken)
    {
        Trace.WriteLine($"{GetType().Name}({dataSourceConfiguration.DatabaseConfiguration!.DatabaseName}).{nameof(Resume)} >>");
        ArgumentNullException.ThrowIfNull(Database);
        dataSyncService?.Start(Database, cancellationToken);
        Trace.WriteLine($"{GetType().Name}({dataSourceConfiguration.DatabaseConfiguration!.DatabaseName}).{nameof(Resume)} <<");
    }

    public async Task<Database> GetDatabaseAsync()
    {
        if(Database == null)
        {
            if(!Database.Exists(DatabaseName, databaseConfiguration.Directory) && databaseSeedService != null)
            {
                Trace.WriteLine($"{nameof(CouchbaseDataSource)}.{nameof(GetDatabaseAsync)} - copying the bundled database:{DatabaseName} to the default directory at {databaseConfiguration.Directory}.");
                await databaseSeedService.CopyDatabaseAsync(databaseConfiguration.Directory);
                Database = new Database(DatabaseName, databaseConfiguration);
            }
            else
            {
                Trace.WriteLine($"{nameof(CouchbaseDataSource)}.{nameof(GetDatabaseAsync)} - opening database:{DatabaseName} in the default directory at {databaseConfiguration.Directory}");
                Database = new Database(DatabaseName, databaseConfiguration);        
                databaseListenerToken = Database.GetDefaultCollection().AddChangeListener(TraceDatabaseChange);
                documentListenerToken = Database.GetDefaultCollection().AddDocumentChangeListener("",TraceDocumentChange);
            }

            CreateDatabaseIndexes();
            await StartAsync(CancellationToken.None);
        }

        return Database;
    }

    private async Task StartAsync(CancellationToken cancellationToken)
    {
        Trace.WriteLine($"{GetType().Name}({dataSourceConfiguration.DatabaseConfiguration!.DatabaseName}).{nameof(StartAsync)} >>");
        var db = await GetDatabaseAsync();

        if(dataSyncService == null && dataSourceConfiguration.SyncConfiguration != null)
        {
            dataSyncService = new DataSyncService(dataSourceConfiguration.SyncConfiguration);
            dataSyncService.Start(db, cancellationToken);
        }
        Trace.WriteLine($"{GetType().Name}({dataSourceConfiguration.DatabaseConfiguration!.DatabaseName}).{nameof(StartAsync)} <<");
    }

    private void CreateDatabaseIndexes()
    {
        if(dataSourceConfiguration.DatabaseConfiguration!.Indexes != null && 
            dataSourceConfiguration.DatabaseConfiguration!.Indexes.Count() > 0 && 
            dataSourceConfiguration.DatabaseConfiguration!.IndexName != null)
        {
            var indexes = dataSourceConfiguration.DatabaseConfiguration!.Indexes;
            var valueIndexItems = new List<IValueIndexItem>();
            for(int i = 0; i < indexes.Count(); ++i)
            {
                if(!string.IsNullOrEmpty(indexes[i]))
                {
                    valueIndexItems.Add(ValueIndexItem.Expression(Expression.Property(indexes[i])));
                };
            }
            
            if(valueIndexItems.Count > 0)
            {
                Database!.GetDefaultCollection().CreateIndex(
                    dataSourceConfiguration.DatabaseConfiguration!.IndexName!, 
                    IndexBuilder.ValueIndex(valueIndexItems.ToArray()));
            }
        }
    }

    public void Dispose()
    {
        dataSyncService?.Dispose();
        dataSyncService = null;

        if(Database != null && databaseListenerToken.HasValue)
        {
            Database?.GetDefaultCollection().RemoveChangeListener(databaseListenerToken.Value);
            databaseListenerToken = null;
        }

        if(Database != null && documentListenerToken.HasValue)
        {
            Database?.GetDefaultCollection().RemoveChangeListener(documentListenerToken.Value);
            documentListenerToken = null;
        }

        Database?.Close();
        Database = null;
    }

    private void TraceDocumentChange(object? sender, DocumentChangedEventArgs e)
    {
        Trace.WriteLine($"{GetType().Name}.{nameof(TraceDocumentChange)}: Document with ID {e.DocumentID} changed. Collection: {e.Collection.Name}");
    }

    private void TraceDatabaseChange(object? sender, CollectionChangedEventArgs e)
    {
        var documentIdsList = string.Join(',', e.DocumentIDs);
        Trace.WriteLine($"{GetType().Name}.{nameof(TraceDocumentChange)}: {e.Database.Name} database changed {e.Collection.Count} items: [{documentIdsList}]");
    }
}
