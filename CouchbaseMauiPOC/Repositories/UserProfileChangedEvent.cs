using CouchbaseMauiPOC.Models;

namespace CouchbaseMauiPOC.Repositories;

public class QueryResultsChangedEventArgs<TDataEntity> : EventArgs where TDataEntity : DataEntity
{
    public List<TDataEntity>? DataEntities { get; private set;}
    public Exception? Exception {get; private set;}

    public QueryResultsChangedEventArgs(List<TDataEntity>? dataEntities, Exception? exception)
    {
        DataEntities = dataEntities;
        Exception = exception;
    }
}

public delegate void UserProfileQueryResultsChangedEvent(QueryResultsChangedEventArgs<UserProfile> args);
public delegate void UniversityQueryResultsChangedEvent(QueryResultsChangedEventArgs<University> args);
