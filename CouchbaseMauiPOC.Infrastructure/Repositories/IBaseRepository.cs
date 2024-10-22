namespace CouchbaseMauiPOC.Infrastructure.Repositories;

public interface IBaseRepository : IDisposable
{
    string? Path { get; }
    Task GetAsync(string id);
}
