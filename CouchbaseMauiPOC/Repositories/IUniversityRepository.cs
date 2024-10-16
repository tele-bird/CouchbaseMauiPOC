using CouchbaseMauiPOC.Models;

namespace CouchbaseMauiPOC.Repositories;

public interface IUniversityRepository : IBaseRepository
{
    // Task<List<University>> All();
    event UniversityQueryResultsChangedEvent? UniversityResultsChanged;

    Task<University?> GetLocalAsync(string id);

    Task StartsWith(string? name = null, string? country = null);

    // Task<List<University>> SearchByName(string? name = null, string? country = null);

    Task<string?> SaveAsync(University university);

    Task<bool> DeleteAsync(University university);
}
