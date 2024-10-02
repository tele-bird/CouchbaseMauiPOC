namespace CouchbaseMauiPOC.Repositories;

public interface IRepository<T, K> : IDisposable
{
    T? Get(K id);
    bool Save(T obj);
}
