using Couchbase.Lite.Query;
using CouchbaseMauiPOC.Models;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.Repositories;

public class UniversityRepository : BaseRepository, IUniversityRepository
{
    public UniversityRepository(IDatabaseSeedService databaseSeedService)
        : base(databaseSeedService, "universities")
        {
        }

    public async Task<List<University>> SearchByName(string name, string? country = null)
    {
        List<University> universities = new List<University>();
        var database = await GetDatabaseAsync();
        if (database != null)
        {
            var whereQueryExpression = Function.Lower(Expression.Property("name")).Like(Expression.String($"%{name.ToLower()}%"));
            if (!string.IsNullOrEmpty(country))
            {
                var countryQueryExpression = Function.Lower(Expression.Property("country")).Like(Expression.String($"%{country.ToLower()}%"));
                whereQueryExpression = whereQueryExpression.And(countryQueryExpression);
            }

            var query = QueryBuilder.Select(SelectResult.All())
                .From(DataSource.Collection(database.GetDefaultCollection()))
                .Where(whereQueryExpression);
                
            var results = query.Execute().AllResults();
            if (results?.Count > 0)
            {
                foreach (var result in results)
                {
                    var dictionary = result.GetDictionary("_default");
                    if (dictionary != null)
                    {
                        var university = new University
                        {
                            Name = dictionary.GetString("name"),
                            Country = dictionary.GetString("country")
                        };
                        
                        universities.Add(university);
                    }
                }
            }
        }
        
        return universities;
     }
}
