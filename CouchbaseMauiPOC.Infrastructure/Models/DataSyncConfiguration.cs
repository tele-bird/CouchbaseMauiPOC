using System;
using Couchbase.Lite.Sync;

namespace CouchbaseMauiPOC.Infrastructure.Models;

public class DataSyncConfiguration
{
    public string? Username {get; set;}
    public string? Password {get; set;}
    public string? EndpointUrl {get; set;}
    public ReplicatorType ReplicatorType {get; set;}
    public bool IsContinuous {get; set;}
}
