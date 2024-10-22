using Couchbase.Lite.DI;

namespace CouchbaseMauiPOC.Console;

public class ConsoleDirectoryResolver : IDefaultDirectoryResolver
{
    private readonly string defaultDirectory;

    public ConsoleDirectoryResolver(string defaultDirectory)
    {
        this.defaultDirectory = defaultDirectory;
    }

    public string DefaultDirectory()
    {
        return defaultDirectory;
    }
}
