using System.Diagnostics;
using Couchbase.Lite;
using Couchbase.Lite.DI;
using Couchbase.Lite.Query;
using Couchbase.Lite.Sync;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.Repositories;

public class DatabaseManager : IDisposable
{
    private readonly IDatabaseSeedService databaseSeedService;
    private readonly string databaseName;
    private readonly Uri remoteSyncUrl;

    private Replicator? replicator;
    private ListenerToken? replicatorListenerToken;
    private ListenerToken? databaseListenerToken;
    private ListenerToken? documentListenerToken;
    public Database? Database { get; private set; }

    internal DatabaseManager(IDatabaseSeedService databaseSeedService, string databaseName)
    {
        this.databaseSeedService = databaseSeedService;
        this.databaseName = databaseName;

        if(DeviceInfo.Current.Platform == DevicePlatform.Android)
        {
            // note: user "10.0.2.2" when using an Android emulator
            remoteSyncUrl = new Uri("wss://ofw9ujauhjnzsf1f.apps.cloud.couchbase.com:4984/userprofile-endpoint");
        }
        else
        {
            // note: user 'localhost' when using an iOS simulator
            remoteSyncUrl = new Uri("wss://ofw9ujauhjnzsf1f.apps.cloud.couchbase.com:4984/userprofile-endpoint");
        }
    }

    public async Task<Database> GetDatabaseAsync()
    {
        if(Database == null)
        {
            if(databaseName == "userprofile")
            {
                var defaultDirectory = Service.GetInstance<IDefaultDirectoryResolver>()!.DefaultDirectory();
                var databaseConfig = new DatabaseConfiguration
                {
                    Directory = Path.Combine(defaultDirectory, AppInstance.User!.Username!)
                };

                Trace.WriteLine($"Creating {databaseName} database at path: {databaseConfig.Directory}");
                Database = new Database(databaseName, databaseConfig);
                Trace.WriteLine($"{databaseName} database created at path: {Database.Path}");
                
                databaseListenerToken = Database.GetDefaultCollection().AddChangeListener(TraceDatabaseChange);
                documentListenerToken = Database.GetDefaultCollection().AddDocumentChangeListener("",TraceDocumentChange);
            }
            else if(databaseName == "universities")
            {
                var options = new DatabaseConfiguration();

                var defaultDirectoryResolver = Service.GetInstance<IDefaultDirectoryResolver>();
                if(defaultDirectoryResolver == null)
                {
                    throw new Exception($"No {nameof(IDefaultDirectoryResolver)} implementation was registered in the {typeof(Service).FullName}.");
                }
                
                var defaultDirectory = defaultDirectoryResolver.DefaultDirectory();
                options.Directory = defaultDirectory;
                if(!Database.Exists(databaseName, defaultDirectory))
                {
                    await databaseSeedService.CopyDatabaseAsync(defaultDirectory);
                    Database = new Database(databaseName, options);
                    CreateUniversitiesDatabaseIndex();
                }
                else
                {
                    Database = new Database(databaseName, options);
                }
            }
        }

        return Database!;
    }

    private void TraceDocumentChange(object? sender, DocumentChangedEventArgs e)
    {
        Trace.WriteLine($"Document with ID {e.DocumentID} changed. Collection: {e.Collection.Name}");
    }

    private void TraceDatabaseChange(object? sender, CollectionChangedEventArgs e)
    {
        var documentIdsList = string.Join(',', e.DocumentIDs);
        Trace.WriteLine($"{e.Database.Name} database changed {e.Collection.Count} items: [{documentIdsList}]");
    }

    private void CreateUniversitiesDatabaseIndex()
    {
        Database!.GetDefaultCollection().CreateIndex("NameLocationIndex", IndexBuilder.ValueIndex(
            ValueIndexItem.Expression(Expression.Property("name")),
            ValueIndexItem.Expression(Expression.Property("location"))
        ));
    }

    public async Task StartReplicationAsync(
        string username,
        string password,
        string[] channels,
        ReplicatorType replicatorType = ReplicatorType.PushAndPull,
        bool continuous = true)
    {
        var database = await GetDatabaseAsync();
        var targetUrlEndpoint = new URLEndpoint(remoteSyncUrl);
        var configuration = new ReplicatorConfiguration(targetUrlEndpoint)
        {
            ReplicatorType = replicatorType,
            Continuous = continuous,
            Authenticator = new BasicAuthenticator(username, password)
            // Channels = channels?.Select(x => $"channel.{x}").ToArray()
        };

        // this is how you would create a sync subcription (filter):
        // var collectionConfiguration = new CollectionConfiguration
        // {
        //      PullFilter = MyPullFilter,
        //      PushFilter = MyPushFilter
        // };
        // configuration.AddCollection(database.GetDefaultCollection(), collectionConfiguration);

        // instead we will not create a sync subscription (filter)
        configuration.AddCollection(database.GetDefaultCollection());

        replicator = new Replicator(configuration);
        replicatorListenerToken = replicator.AddChangeListener(OnReplicatorUpdate);
        replicator.Start();
    }

    // private bool MyPullFilter(Document document, DocumentFlags documentFlags)
    // {
    //     return document.Keys.Contains("somekey") && document["somekey"].Equals("somevalue");
    // }

    // private bool MyPushFilter(Document document, DocumentFlags documentFlags)
    // {
    //     return document.Keys.Contains("somekey") && document["somekey"].Equals("somevalue");
    // }

    void OnReplicatorUpdate(object? sender, ReplicatorStatusChangedEventArgs e)
    {
        if(e.Status.Error != null)
        {
            Trace.WriteLine($"Replicator error: {e.Status.Error.GetType().Name}: {e.Status.Error.Message}");
        }

        switch (e.Status.Activity)
        {
            case ReplicatorActivityLevel.Busy:
            {
                var message = e.Status.Error != null ? e.Status.Error.Message : null;
                Trace.WriteLine($"Replicator busy: {e.Status.Progress.Completed}/{e.Status.Progress.Total}");
                break;
            }
            default:
            {
                Trace.WriteLine($"Replicator in {e.Status.Activity} state.");
                break;
            }
         }
    }

    public void Dispose()
    {
        if(replicator != null)
        {
            StopReplication();

            while(true)
            {
                if(replicator.Status.Activity == ReplicatorActivityLevel.Stopped)
                {
                    break;
                }
            }

            replicator.Dispose();
        }

        if(Database != null)
        {
            Database.Close();
            Database = null;
        }
    }

    private void StopReplication()
    {
        if(replicator == null)
        {
            throw new ArgumentNullException(nameof(replicator));
        }

        if(replicatorListenerToken.HasValue)
        {
            replicator.RemoveChangeListener(replicatorListenerToken.Value);
            replicatorListenerToken = null;
        }

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

        replicator.Stop();
    }
}
