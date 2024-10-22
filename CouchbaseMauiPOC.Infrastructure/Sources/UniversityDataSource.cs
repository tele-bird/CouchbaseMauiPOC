using Couchbase.Lite;
using Couchbase.Lite.DI;
using CouchbaseMauiPOC.Infrastructure.Services;
using Couchbase.Lite.Query;
using System.Diagnostics;

namespace CouchbaseMauiPOC.Infrastructure.Sources;

public class UniversityDataSource : DataSource
{
    private readonly IDatabaseSeedService databaseSeedService;
    private readonly DatabaseConfiguration databaseConfiguration;

    private DataSyncService? dataSyncService;
    private ListenerToken? databaseListenerToken;
    private ListenerToken? documentListenerToken;

    public UniversityDataSource(
        IDatabaseSeedService databaseSeedService)
        : base("universities")
    {
        this.databaseSeedService = databaseSeedService;
        var defaultDirectoryResolver = Service.GetInstance<IDefaultDirectoryResolver>();
        Trace.WriteLine($"{GetType().Name}.{nameof(UniversityDataSource)} - Found an IDefaultDirectoryResolver: {defaultDirectoryResolver!.GetType().FullName}");
        databaseConfiguration = new DatabaseConfiguration()
        {
            Directory = defaultDirectoryResolver!.DefaultDirectory()
        };
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
        var db = await GetDatabaseAsync();

        if(dataSyncService != null)
        {
            await dataSyncService.StartAsync(db, cancellationToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if(dataSyncService != null)
        {
            await dataSyncService.StopAsync(cancellationToken);
        }
    }

    public override async Task<Database> GetDatabaseAsync()
    {
        if(Database == null)
        {
            if(!Database.Exists(DatabaseName, databaseConfiguration.Directory))
            {
                Trace.WriteLine($"{nameof(UniversityDataSource)}.{nameof(GetDatabaseAsync)} - database:{DatabaseName} does not exist in the path: {databaseConfiguration.Directory}");
                await databaseSeedService.CopyDatabaseAsync(databaseConfiguration.Directory);
                Database = new Database(DatabaseName, databaseConfiguration);
                CreateUniversitiesDatabaseIndex();
            }
            else
            {
                Trace.WriteLine($"{nameof(UniversityDataSource)}.{nameof(GetDatabaseAsync)} - database:{DatabaseName} found in the path: {databaseConfiguration.Directory}");
                Database = new Database(DatabaseName, databaseConfiguration);
            }

            databaseListenerToken = Database.GetDefaultCollection().AddChangeListener(TraceDatabaseChange);
            documentListenerToken = Database.GetDefaultCollection().AddDocumentChangeListener("",TraceDocumentChange);
        }

        return Database;
    }

    public override void Dispose()
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

    private void CreateUniversitiesDatabaseIndex()
    {
        Database!.GetDefaultCollection().CreateIndex("NameLocationIndex", IndexBuilder.ValueIndex(
            ValueIndexItem.Expression(Expression.Property("name")),
            ValueIndexItem.Expression(Expression.Property("location"))
        ));
    }
}
