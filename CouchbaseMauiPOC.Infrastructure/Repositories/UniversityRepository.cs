using System.Diagnostics;
using Couchbase.Lite;
using Couchbase.Lite.Query;
using CouchbaseMauiPOC.Infrastructure.Events;
using CouchbaseMauiPOC.Infrastructure.Models;
using CouchbaseMauiPOC.Infrastructure.Services;

namespace CouchbaseMauiPOC.Infrastructure.Repositories;

public class UniversityRepository : BaseRepository, IUniversityRepository
{
    private ListenerToken? universityQueryToken;
    private IQuery? universityQuery;
    private IQuery? UniversityQuery
    {
        get
        {
            return universityQuery;
        }
        set
        {
            // dispose of previous query:
            if(universityQuery != null && universityQueryToken.HasValue)
            {
                universityQuery.RemoveChangeListener(universityQueryToken.Value);
                universityQueryToken = null;
                universityQuery.Dispose();
            }

            // set new value:
            universityQuery = value;

            // subscribe to query changes:
            universityQueryToken = universityQuery?.AddChangeListener(HandleQueryResultChanged);
        }
    }

    private ListenerToken? startsWithQueryToken;
    private IQuery? startsWithQuery;
    private IQuery? StartsWithQuery
    {
        get
        {
            return startsWithQuery;
        }
        set
        {
            // dispose of previous query:
            if(startsWithQuery != null && startsWithQueryToken.HasValue)
            {
                startsWithQuery.RemoveChangeListener(startsWithQueryToken.Value);
                startsWithQueryToken = null;
                startsWithQuery.Dispose();
            }

            // set new value:
            startsWithQuery = value;

            // subscribe to query changes:
            startsWithQueryToken = startsWithQuery?.AddChangeListener(HandleQueryResultsChanged);
        }
    }

    public event UniversityQueryResultsChangedEvent? UniversityResultsChanged;
    public event UniversityQueryResultChangedEvent? UniversityResultChanged;

    public UniversityRepository(Sources.UniversityDataSource universityDataSource)
        : base(universityDataSource)
    {
    }

    private List<University> ExtractResults(List<Result> results)
    {
        var universities = new List<University>();
        foreach(var result in results)
        {
            // var jsonResult = result.ToJSON();
            // Trace.WriteLine($"result[{rowNum}]: {jsonResult}");
            var dictionary = result.GetDictionary("_default");
            ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));
            // var rowDebugString = dictionary.ToJSON();
            // Trace.WriteLine($"result[{rowNum}]: {rowDebugString}");
            var university = new University
            {
                Id = result.GetString("id"),
                Name = dictionary.GetString("name"),
                Country = dictionary.GetString("country"),
                AlphaTwoCode = dictionary.GetString("alpha_two_code")
            };

            universities.Add(university);
        }

        return universities;
    }

    // public async Task<List<University>> All()
    // {
    //     List<University> universities = new List<University>();
    //     var database = await GetDatabaseAsync();
    //     if (database != null)
    //     {
    //         var whereQueryExpression = Function.Lower(Expression.Property("type")).Is(Expression.String("university"));
    //         var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID), SelectResult.All())
    //             .From(DataSource.Collection(database.GetDefaultCollection()))
    //             .Where(whereQueryExpression)
    //             .OrderBy(Ordering.Expression(Meta.ID).Ascending());
                
    //         var results = query.Execute().AllResults();
    //         ExtractResults(results, (university) =>
    //         {
    //             universities.Add(university);
    //         });
    //     }
        
    //     return universities;
    // }

    // public async Task<University?> GetLocalAsync(string id)
    // {
    //     var database = await GetDatabaseAsync();
    //     ArgumentNullException.ThrowIfNull(database, nameof(database));
    //     var document = database.GetDefaultCollection().GetDocument(id);
    //     ArgumentNullException.ThrowIfNull(document, nameof(document));
    //     return new University
    //     {
    //         Id = document.Id,
    //         Name = document.GetString("name"),
    //         Country = document.GetString("country"),
    //         AlphaTwoCode = document.GetString("alpha_two_code")
    //     };
    // }

    public override async Task GetAsync(string id)
    {
        var database = await GetDatabaseAsync();
        ArgumentNullException.ThrowIfNull(database, nameof(database));
        UniversityQuery = QueryBuilder
            .Select(SelectResult.Expression(Meta.ID), SelectResult.All())
            .From(Couchbase.Lite.Query.DataSource.Collection(database.GetDefaultCollection()))
            .Where(Expression.Property("type").EqualTo(Expression.String("university"))
            .And(Meta.ID.EqualTo(Expression.String(id))));
     }

    public async Task StartsWith(string? name = null, string? country = null)
    {
        var database = await GetDatabaseAsync();
        ArgumentNullException.ThrowIfNull(database, nameof(database));
        var whereQueryExpression = Function.Lower(Expression.Property("type")).Is(Expression.String("university"));
        if(!string.IsNullOrEmpty(name))
        {
            var nameQueryExpression = Function.Lower(Expression.Property("name")).Like(Expression.String($"{name.ToLower()}%"));
            whereQueryExpression = whereQueryExpression.And(nameQueryExpression);
        }
        if (!string.IsNullOrEmpty(country))
        {
            var countryQueryExpression = Function.Lower(Expression.Property("country")).Like(Expression.String($"{country.ToLower()}%"));
            whereQueryExpression = whereQueryExpression.And(countryQueryExpression);
        }
        StartsWithQuery = QueryBuilder.Select(SelectResult.Expression(Meta.ID), SelectResult.All())
            .From(Couchbase.Lite.Query.DataSource.Collection(database.GetDefaultCollection()))
            .Where(whereQueryExpression)
            .OrderBy(Ordering.Property("name").Ascending());
            // .OrderBy(Ordering.Expression(Meta.ID).Ascending());
    }

    private void HandleQueryResultChanged(object? sender, QueryChangedEventArgs e)
    {
        var resultChangedEventArgs = new QueryResultChangedEventArgs<University>();
        if(e.Error != null)
        {
            resultChangedEventArgs.Exception = e.Error;
        }
        else
        {
            try
            {
                var resultsList = e.Results.AllResults();
                Trace.WriteLine($"{nameof(UniversityRepository)}.{nameof(HandleQueryResultChanged)} >> got {resultsList.Count} results");
                var universities = ExtractResults(resultsList);
                if(universities.Count > 1)
                {
                    throw new Exception($"Unexpected scenario: query returned {universities.Count} entities.");
                }
                if(universities.Count == 1)
                {
                    resultChangedEventArgs.DataEntity = universities[0];
                }
            }
            catch(Exception exc)
            {
                resultChangedEventArgs.Exception = exc;
            }
        }

        UniversityResultChanged?.Invoke(resultChangedEventArgs);
    }

    private void HandleQueryResultsChanged(object? sender, QueryChangedEventArgs e)
    {
        var resultsChangedEventArgs = new QueryResultsChangedEventArgs<University>();
        if(e.Error != null)
        {
            resultsChangedEventArgs.Exception = e.Error;
        }
        else
        {
            try
            {
                var resultsList = e.Results.AllResults();
                Trace.WriteLine($"{nameof(UniversityRepository)}.{nameof(HandleQueryResultsChanged)} >> got {resultsList.Count} results");
                resultsChangedEventArgs.DataEntities = ExtractResults(resultsList);
            }
            catch(Exception exc)
            {
                resultsChangedEventArgs.Exception = exc;
            }
        }

        UniversityResultsChanged?.Invoke(resultsChangedEventArgs);
    }

    // public async Task<List<University>> SearchByName(string? name = null, string? country = null)
    // {
    //     List<University> universities = new List<University>();
    //     var database = await GetDatabaseAsync();
    //     if (database != null)
    //     {
    //         // always filter on type = "university"
    //         var whereQueryExpression = Function.Lower(Expression.Property("type")).Is(Expression.String("university"));

    //         // filter on name, if provided
    //         if(!string.IsNullOrEmpty(name))
    //         {
    //             var nameQueryExpression = Function.Lower(Expression.Property("name")).Like(Expression.String($"%{name.ToLower()}%"));;
    //             whereQueryExpression = whereQueryExpression.And(nameQueryExpression);
    //         }

    //         // filter on country if provided
    //         if (!string.IsNullOrEmpty(country))
    //         {
    //             var countryQueryExpression = Function.Lower(Expression.Property("country")).Like(Expression.String($"%{country.ToLower()}%"));
    //             whereQueryExpression = whereQueryExpression.And(countryQueryExpression);
    //         }

    //         // execute the query
    //         var query = QueryBuilder.Select(SelectResult.All())
    //             .From(DataSource.Collection(database.GetDefaultCollection()))
    //             .Where(whereQueryExpression)
    //             .OrderBy(Ordering.Property("name").Ascending());
                
    //         var results = query.Execute().AllResults();
    //         ExtractResults(results, (university) =>
    //         {
    //             universities.Add(university);
    //         });
    //     }
        
    //     return universities;
    //  }

     public async Task<string?> SaveAsync(University university)
     {
        ArgumentNullException.ThrowIfNull(university);
            var mutableDocument = university.Id != null ? new MutableDocument(university.Id) : new MutableDocument();
            mutableDocument.SetString("name", university.Name);
            mutableDocument.SetString("country", university.Country);
            mutableDocument.SetString("alpha_two_code", university.AlphaTwoCode);
            mutableDocument.SetString("type", university.Type);
            
            var database = await GetDatabaseAsync();
            var collection = database.GetDefaultCollection();
            if(collection.Save(mutableDocument, ConcurrencyControl.LastWriteWins))
            {
                return mutableDocument.Id;
            }
            else
            {
                return null;
            }        
     }

     public async Task<bool> DeleteAsync(University university)
     {
        var database = await GetDatabaseAsync();
        ArgumentNullException.ThrowIfNull(database, nameof(database));
        ArgumentNullException.ThrowIfNull(university.Id, nameof(university.Id));
        var document = database.GetDefaultCollection().GetDocument(university.Id);
        if(document == null) return false;
        return database.GetDefaultCollection().Delete(document, ConcurrencyControl.LastWriteWins);
     }

    public override void Dispose()
    {
        UniversityQuery = null;
        StartsWithQuery = null;
        base.Dispose();
    }
}
