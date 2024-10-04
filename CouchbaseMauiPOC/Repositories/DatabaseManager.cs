using Couchbase.Lite;
using Couchbase.Lite.DI;
using Couchbase.Lite.Query;
using CouchbaseMauiPOC.Services;

namespace CouchbaseMauiPOC.Repositories;

internal class DatabaseManager
{
    private readonly IServiceProvider serviceProvider;
    private readonly string databaseName;

    Database? database;

    internal DatabaseManager(IServiceProvider serviceProvider, string databaseName)
    {
        this.serviceProvider = serviceProvider;
        this.databaseName = databaseName;
    }

    public async Task<Database> GetDatabaseAsync()
    {
        if(database == null)
        {
            if(databaseName == "userprofile")
            {
                var databaseConfig = new DatabaseConfiguration
                {
                    Directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppInstance.User!.Username!)
                };

                database = new Database(databaseName, databaseConfig);
            }
            else if(databaseName == "universities")
            {
                var options = new DatabaseConfiguration();

                var defaultDirectoryResolver = Service.GetInstance<IDefaultDirectoryResolver>();
                if(defaultDirectoryResolver == null)
                {
                    throw new Exception($"No {nameof(IDefaultDirectoryResolver)} implementation was registered in the {typeof(Service).FullName}.");
                }
                
                var defaultDirectory = defaultDirectoryResolver.DefaultDirectory();
                options.Directory = defaultDirectory;
                if(!Database.Exists(databaseName, defaultDirectory))
                {
                    var DatabaseSeedService = serviceProvider.GetRequiredService<IDatabaseSeedService>();
                    if(DatabaseSeedService != null)
                    {
                        await DatabaseSeedService.CopyDatabaseAsync(defaultDirectory);
                        database = new Database(databaseName, options);
                        CreateUniversitiesDatabaseIndex();
                    }
                }
                else
                {
                    database = new Database(databaseName, options);
                }
            }
        }

        return database!;
    }

    private void CreateUniversitiesDatabaseIndex()
    {
        database!.GetDefaultCollection().CreateIndex("NameLocationIndex", IndexBuilder.ValueIndex(
            ValueIndexItem.Expression(Expression.Property("name")),
            ValueIndexItem.Expression(Expression.Property("location"))
        ));
    }
}
