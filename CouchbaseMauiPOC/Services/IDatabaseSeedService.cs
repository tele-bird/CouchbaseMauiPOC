namespace CouchbaseMauiPOC.Services;

public interface IDatabaseSeedService
{
    Task CopyDatabaseAsync(string targetDirectoryPath);
}
