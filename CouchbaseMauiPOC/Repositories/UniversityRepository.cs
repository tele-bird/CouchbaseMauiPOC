using System.Diagnostics;
using Couchbase.Lite;
using Couchbase.Lite.Query;
using CouchbaseMauiPOC.Extensions;
using CouchbaseMauiPOC.Models;
using CouchbaseMauiPOC.Services;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

namespace CouchbaseMauiPOC.Repositories;

public class UniversityRepository : BaseRepository, IUniversityRepository
{
    private ListenerToken? queryToken;
    private ListenerToken? QueryToken
    {
        get
        {
            return queryToken;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(databaseManager.Database, nameof(databaseManager.Database));
            if(queryToken.HasValue)
            {
                databaseManager.Database.GetDefaultCollection().RemoveChangeListener(queryToken.Value);
            }
            queryToken = value;
        }
    }

    public event UniversityQueryResultsChangedEvent? UniversityResultsChanged;

    public UniversityRepository(IDatabaseSeedService databaseSeedService)
        : base(databaseSeedService, "universities")
    {
    }

    private void ExtractResults(List<Result>? results, Action<List<University>?, Exception?> extractedAction)
    {
        List<University>? universities = null;
        Exception? exception = null;
        try
        {
            ArgumentNullException.ThrowIfNull(results, nameof(results));
            universities = new List<University>();
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
        }
        catch(Exception exc)
        {
            exception = exc;
        }
        finally
        {
            extractedAction.Invoke(universities, exception);
        }
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

    public async Task<University?> GetLocalAsync(string id)
    {
        var database = await GetDatabaseAsync();
        ArgumentNullException.ThrowIfNull(database, nameof(database));
        var document = database.GetDefaultCollection().GetDocument(id);
        ArgumentNullException.ThrowIfNull(document, nameof(document));
        return new University
        {
            Id = document.Id,
            Name = document.GetString("name"),
            Country = document.GetString("country"),
            AlphaTwoCode = document.GetString("alphaTwoCode")
        };
    }

    public async Task StartsWith(string? name = null, string? country = null)
    {
        var database = await GetDatabaseAsync();
        ArgumentNullException.ThrowIfNull(database, nameof(database));
        var whereQueryExpression = Function.Lower(Expression.Property("type")).Is(Expression.String("university"));
        if(!string.IsNullOrEmpty(name))
        {
            var nameQueryExpression = Function.Lower(Expression.Property("name")).Like(Expression.String($"{name.ToLower()}%"));;
            whereQueryExpression = whereQueryExpression.And(nameQueryExpression);
        }
        if (!string.IsNullOrEmpty(country))
        {
            var countryQueryExpression = Function.Lower(Expression.Property("country")).Like(Expression.String($"{country.ToLower()}%"));
            whereQueryExpression = whereQueryExpression.And(countryQueryExpression);
        }
        QueryToken = QueryBuilder.Select(SelectResult.Expression(Meta.ID), SelectResult.All())
            .From(DataSource.Collection(database.GetDefaultCollection()))
            .Where(whereQueryExpression)
            .OrderBy(Ordering.Property("name").Ascending())
                .AddChangeListener(HandleQueryResultsChanged);
    }

    private void HandleQueryResultsChanged(object? sender, QueryChangedEventArgs e)
    {
        if(e.Error != null)
        {
            Trace.WriteLine($"Live query change listener received error: {e.Error.GetType().Name}: {e.Error.Message}");
        }
        else if (e?.Results != null)
        {
            var resultsList = e.Results.AllResults();
            Trace.WriteLine($"{nameof(UniversityRepository)}.{nameof(HandleQueryResultsChanged)} >> got {resultsList.Count} results");
            ExtractResults(resultsList, (universities, exception) =>
            {
                UniversityResultsChanged?.Invoke(new QueryResultsChangedEventArgs<University>(universities, exception));
            });
        }
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
            mutableDocument.SetString("alphaTwoCode", university.AlphaTwoCode);
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
        ArgumentNullException.ThrowIfNull(document, nameof(document));
        return database.GetDefaultCollection().Delete(document, ConcurrencyControl.LastWriteWins);
     }

    public override void Dispose()
    {
        QueryToken = null;
        base.Dispose();
    }
}
