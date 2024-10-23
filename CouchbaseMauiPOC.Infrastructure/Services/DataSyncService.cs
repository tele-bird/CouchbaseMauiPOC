using Couchbase.Lite;
using Couchbase.Lite.Sync;
using CouchbaseMauiPOC.Infrastructure.Models;

namespace CouchbaseMauiPOC.Infrastructure.Services;

public class DataSyncService : IDisposable
{
    private readonly ReplicatorConfiguration replicatorConfiguration;

    private Replicator? replicator;
    private ListenerToken? replicatorListenerToken;

    public DataSyncService(DataSyncConfiguration dataSyncConfiguration)
    {
        replicatorConfiguration = new ReplicatorConfiguration(new URLEndpoint(new Uri(dataSyncConfiguration.EndpointUrl!)))
        {
            ReplicatorType = dataSyncConfiguration.ReplicatorType,
            Continuous = dataSyncConfiguration.IsContinuous,
            Authenticator = new BasicAuthenticator(dataSyncConfiguration.Username!, dataSyncConfiguration.Password!),
            // Channels = channels
        };
    }

    public void Start(Database database, CancellationToken cancellationToken)
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
    }

    private void OnReplicatorStatusChanged(object? sender, ReplicatorStatusChangedEventArgs args)
    {
        Console.WriteLine($"{GetType().Name}.{nameof(OnReplicatorStatusChanged)} - Activity: {args.Status.Activity}");
        if (args.Status.Error != null)
        {
            Console.WriteLine($"{GetType().Name}.{nameof(OnReplicatorStatusChanged)} - Error: {args.Status.Error}");
        }
        else
        {
            Console.WriteLine($"{GetType().Name}.{nameof(OnReplicatorStatusChanged)} - ProgressTotal: {args.Status.Progress.Total} ProgressCompleted: {args.Status.Progress.Completed}");
        }
    }

    public void Stop(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(replicator);

        if(replicator.Status.Activity != ReplicatorActivityLevel.Stopped)
        {
            replicator.Stop();
        }
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
