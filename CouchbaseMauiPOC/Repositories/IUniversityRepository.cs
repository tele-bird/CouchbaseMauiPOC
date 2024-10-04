using CouchbaseMauiPOC.Models;

namespace CouchbaseMauiPOC.Repositories;

public interface IUniversityRepository : IBaseRepository
{
    Task<List<University>> SearchByName(string name, string? country = null);
}
