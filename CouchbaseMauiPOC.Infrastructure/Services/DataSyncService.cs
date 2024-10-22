using Couchbase.Lite;
using Couchbase.Lite.Sync;

namespace CouchbaseMauiPOC.Infrastructure.Services;

public class DataSyncService : IDisposable
{
    private readonly ReplicatorConfiguration replicatorConfiguration;

    private Replicator? replicator;
    private ListenerToken? replicatorListenerToken;

    public DataSyncService(
        string username,
        string password,
        Uri endpointUrl,
        ReplicatorType replicatorType = ReplicatorType.PushAndPull,
        bool continuous = true)
    {
        replicatorConfiguration = new ReplicatorConfiguration(new URLEndpoint(endpointUrl))
        {
            ReplicatorType = replicatorType,
            Continuous = continuous,
            Authenticator = new BasicAuthenticator(username, password),
            // Channels = channels
        };
    }

    public Task StartAsync(Database database, CancellationToken cancellationToken)
    {
        if(replicator == null)
        {
            replicatorConfiguration.AddCollection(database.GetDefaultCollection());
            replicator = new Replicator(replicatorConfiguration);
            replicatorListenerToken = replicator.AddChangeListener(OnReplicatorStatusChanged);
            replicator.Start();
        }

        if(replicator.Status.Activity == ReplicatorActivityLevel.Stopped)
        {
            replicator.Start();
        }

        return Task.CompletedTask;
    }

    private void OnReplicatorStatusChanged(object? sender, ReplicatorStatusChangedEventArgs args)
    {
        Console.WriteLine($"Activity: {args.Status.Activity}");
        if (args.Status.Error != null)
        {
            Console.WriteLine($"Error :: {args.Status.Error}");
        }
        else
        {
            Console.WriteLine($"Progress :: Total:: {args.Status.Progress.Total} Completed:: {args.Status.Progress.Completed}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(replicator);

        if(replicator.Status.Activity != ReplicatorActivityLevel.Stopped)
        {
            replicator.Stop();
        }
        
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        replicator?.Stop();
        if(replicatorListenerToken.HasValue)
        {
            replicator?.RemoveChangeListener(replicatorListenerToken.Value);
            replicatorListenerToken = null;
        }
        replicator?.Dispose();
        replicator = null;
    }
}
