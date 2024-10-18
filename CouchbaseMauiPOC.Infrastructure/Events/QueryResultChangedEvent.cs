using CouchbaseMauiPOC.Infrastructure.Models;

namespace CouchbaseMauiPOC.Infrastructure.Events;

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
public delegate void UniversityQueryResultChangedEvent(QueryResultChangedEventArgs<University> args);

