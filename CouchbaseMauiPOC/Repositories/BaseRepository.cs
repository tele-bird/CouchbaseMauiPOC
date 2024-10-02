using System.Diagnostics;
using Couchbase.Lite;

namespace CouchbaseMauiPOC.Repositories;

public abstract class BaseRepository<T, K> : IRepository<T, K> where T : class
{
    string DatabaseName {get; set;}
    ListenerToken DatabaseListenerToken {get; set;}

    protected virtual DatabaseConfiguration? DatabaseConfig {get; set;}

    Database? database;
    protected Database Database
    {
        get
        {
            if(database == null)
            {
                database = new Database(DatabaseName, DatabaseConfig);
            }
            return database;
        }
        private set => database = value;
    }

    protected BaseRepository(string databaseName)
    {
        if(string.IsNullOrEmpty(databaseName))
        {
            throw new ArgumentOutOfRangeException("databaseName cannot be null or empty");
        }

        DatabaseName = databaseName;

        DatabaseListenerToken = Database.GetDefaultCollection().AddChangeListener(OnDatabaseChangeEvent);
    }

    private void OnDatabaseChangeEvent(object? sender, CollectionChangedEventArgs e)
    {
        foreach(var documentId in e.DocumentIDs)
        {
            var document = Database.GetDefaultCollection().GetDocument(documentId);
            string message = $"Document (id={documentId}) was ";

            if(document == null)
            {
                message += "deleted";
            }
            else
            {
                message += "added/updated";
            }

            Trace.WriteLine(message);
        }
    }

    public void Dispose()
    {
        DatabaseConfig = null;
        Database.GetDefaultCollection().RemoveChangeListener(DatabaseListenerToken);
        Database.Close();
        database = null;
    }

    public abstract T? Get(K id);

    public abstract bool Save(T obj);
}
