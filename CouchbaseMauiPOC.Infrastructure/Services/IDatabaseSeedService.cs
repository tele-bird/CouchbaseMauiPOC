namespace CouchbaseMauiPOC.Infrastructure.Services;

public interface IDatabaseSeedService
{
    Task CopyDatabaseAsync(string targetDirectoryPath);
}
