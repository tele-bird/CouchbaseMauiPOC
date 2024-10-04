namespace CouchbaseMauiPOC.Services;

public partial class DatabaseSeedService : IDatabaseSeedService
{
    public partial Task CopyDatabaseAsync(string targetDirectoryPath);
}
