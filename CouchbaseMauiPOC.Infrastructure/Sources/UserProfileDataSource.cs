using Couchbase.Lite;
using Couchbase.Lite.DI;
using CouchbaseMauiPOC.Infrastructure.Services;

namespace CouchbaseMauiPOC.Infrastructure.Sources;

public class UserProfileDataSource : DataSource
{
    private readonly string username;
    private readonly string password;
    private readonly DatabaseConfiguration databaseConfiguration;
    private readonly Uri endpointUrl;

    private DataSyncService? dataSyncService;
    private ListenerToken? databaseListenerToken;
    private ListenerToken? documentListenerToken;

    public UserProfileDataSource()
        : base("userprofile")
    {
        username = "myusername";
        password = "mypassword";
        endpointUrl = new Uri("wss://ofw9ujauhjnzsf1f.apps.cloud.couchbase.com:4984/userprofile-endpoint");
        var defaultDirectoryResolver = Service.GetInstance<IDefaultDirectoryResolver>();
        ArgumentNullException.ThrowIfNull(defaultDirectoryResolver);
        databaseConfiguration = new DatabaseConfiguration()
        {
            Directory = defaultDirectoryResolver.DefaultDirectory()
        };
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
        var db = await GetDatabaseAsync();

        if(dataSyncService == null)
        {
            dataSyncService = new DataSyncService(username, password, endpointUrl);
        }

        await dataSyncService.StartAsync(db, cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if(dataSyncService != null)
        {
            await dataSyncService.StopAsync(cancellationToken);
        }
    }

    public override Task<Database> GetDatabaseAsync()
    {
        if(Database == null)
        {
            Database = new Database(DatabaseName, databaseConfiguration);        
            databaseListenerToken = Database.GetDefaultCollection().AddChangeListener(TraceDatabaseChange);
            documentListenerToken = Database.GetDefaultCollection().AddDocumentChangeListener("",TraceDocumentChange);
        }

        return Task.FromResult(Database);
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
}
