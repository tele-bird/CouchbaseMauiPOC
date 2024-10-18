using CouchbaseMauiPOC.Infrastructure.Models;

namespace CouchbaseMauiPOC.Infrastructure.Events;

public class QueryResultsChangedEventArgs<TDataEntity> : EventArgs where TDataEntity : DataEntity
{
    public List<TDataEntity>? DataEntities { get; set;}
    public Exception? Exception {get; set;}

    public QueryResultsChangedEventArgs(List<TDataEntity>? dataEntities = null, Exception? exception = null)
    {
        DataEntities = dataEntities;
        Exception = exception;
    }
}

public delegate void UniversityQueryResultsChangedEvent(QueryResultsChangedEventArgs<University> args);

