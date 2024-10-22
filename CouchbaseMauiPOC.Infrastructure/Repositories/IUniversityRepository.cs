using CouchbaseMauiPOC.Infrastructure.Events;
using CouchbaseMauiPOC.Infrastructure.Models;

namespace CouchbaseMauiPOC.Infrastructure.Repositories;

public interface IUniversityRepository : IBaseRepository
{
    // Task<List<University>> All();
    event UniversityQueryResultsChangedEvent? UniversityResultsChanged;

    event UniversityQueryResultChangedEvent? UniversityResultChanged;

    // Task GetAsync(string universityId);

    // Task<University?> GetLocalAsync(string id);

    Task StartsWith(string? name = null, string? country = null);

    // Task<List<University>> SearchByName(string? name = null, string? country = null);

    Task<string?> SaveAsync(University university);

    Task<bool> DeleteAsync(University university);
}
