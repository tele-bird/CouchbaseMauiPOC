namespace CouchbaseMauiPOC.Repositories;

public interface IBaseRepository : IDisposable
{
    string? Path { get; }
}
