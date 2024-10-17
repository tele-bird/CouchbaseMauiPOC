using CouchbaseMauiPOC.Models;

namespace CouchbaseMauiPOC.Repositories;

public class QueryResultChangedEventArgs<TDataEntity> : EventArgs where TDataEntity : DataEntity
{
    public TDataEntity? DataEntity { get; set;}
    public Exception? Exception {get; set;}

    public QueryResultChangedEventArgs(TDataEntity? dataEntity = null, Exception? exception = null)
    {
        DataEntity = dataEntity;
        Exception = exception;
    }
}

public delegate void UserProfileQueryResultChangedEvent(QueryResultChangedEventArgs<UserProfile> args);

